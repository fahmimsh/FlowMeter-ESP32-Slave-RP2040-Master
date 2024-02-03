#include <Arduino.h>
#include <stdio.h>
#include <FreeRTOS.h>
#include <task.h>
#include <semphr.h>
#include <ModbusSlave.h>
#include <ModbusRTUMaster.h>
#include <Bounce2.h>
#include <SPI.h>
#include <Ethernet2.h>
#include <nanomodbus.h>
#define UNUSED_PARAM(x) ((void)(x))

uint8_t idFlow = 1; IPAddress ip(192, 168, 1, 123);
//uint8_t idFlow = 2; IPAddress ip(192, 168, 1, 124);
IPAddress gateway(192, 168, 1, 1), dns_server(192, 168, 110, 201), subnet(255,255,255,0);
#define STACK_SIZE 200
pin_size_t X[6] = {12, 13, 14, 15, 21, 22};
pin_size_t Y[6] = {2, 3, 6, 7, 10, 11};
pin_size_t pin_led = 25;
const int MaxId = 2;
union mbFloatInt{
  struct {
    float kFact; float capacity; float setPoint; float factorKurang; float liter; float Flowrate; uint16_t timeInterval_OnValve; uint16_t timeInterval_OffValve; uint16_t over_fl_err;
  } values;
  uint16_t words[15];
};
mbFloatInt mbFloat[MaxId], mbFloat_Write[MaxId];
struct CoilBool { bool coil[5]; bool inputDiscrete[2]; };
CoilBool coilBool[MaxId], coilBool_Write[MaxId];
uint8_t countStepMbmaster[MaxId], flagWriteMbMasterRH[MaxId], flagWriteMbMasterCoil[MaxId];
uint8_t startAddresWriteCoil[MaxId], startAddresWriteRH[MaxId], startAddresWriteRH_length[MaxId];

unsigned long timePrevIsConnectTCP;
bool stateLed, IsConnectTCP, mb_flagCoil[12];
uint8_t mb_sizeDiscreateInput = ((sizeof(coilBool[0].inputDiscrete) / sizeof(coilBool[0].inputDiscrete[0])) * MaxId) + (sizeof(X) / sizeof(X[0]) + 1);
uint8_t mb_sizeCoil = ((sizeof(coilBool[0].coil) / sizeof(coilBool[0].coil[0])) * MaxId) + (sizeof(Y) / sizeof(Y[0])) + (sizeof(mb_flagCoil) / sizeof(mb_flagCoil[0]));
uint8_t mb_sizeHoldingRegister = ((sizeof(mbFloat[0].words) / sizeof(mbFloat[0].words[0])) * MaxId);

EthernetServer server(502);
EthernetClient client;
nmbs_t nmbs; nmbs_error err;
Bounce2::Button btn[6];
ModbusRTUMaster mbMaster(Serial1);
Modbus *mb;
StaticTask_t xTaskBuffer_slaveid1, xTaskBuffer_slaveid2, xTaskBuffer_slaveidWrite;
StackType_t xStack_slaveid1[400], xStack_slaveid2[400], xStack_slaveidWrite[400];
SemaphoreHandle_t xSemaphore = NULL;
StaticSemaphore_t xMutexBuffer;

void TaskslaveId1(void *pvParameters), TaskslaveId2(void *pvParameters), TaskslaveIdWrite(void *pvParameters);
void read_slave(uint8_t id);
uint8_t mb_ReadDiscreteInput(uint8_t fc, uint16_t address, uint16_t length);
uint8_t mb_Coil(uint8_t fc, uint16_t address, uint16_t length);
uint8_t mb_HoldRegister(uint8_t fc, uint16_t address, uint16_t length);
int32_t read_ethernet(uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg);
int32_t write_ethernet(const uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg);
void mbTCPpoll();
nmbs_error handle_read_discrete_inputs(uint16_t address, uint16_t quantity, uint8_t *inputs_out, uint8_t unit_id, void *arg);
nmbs_error handle_read_coils(uint16_t address, uint16_t quantity, nmbs_bitfield coils_out, uint8_t unit_id, void *arg);
nmbs_error hadle_write_single_coils(uint16_t address, bool value, uint8_t unit_id, void *arg);
nmbs_error handle_write_multiple_coils(uint16_t address, uint16_t quantity, const uint8_t *coils, uint8_t unit_id, void *arg);
nmbs_error handler_read_holding_registers(uint16_t address, uint16_t quantity, uint16_t *registers_out, uint8_t unit_id, void *arg);
nmbs_error handler_write_single_register(uint16_t address, uint16_t value, uint8_t unit_id, void *arg);
nmbs_error handle_write_multiple_registers(uint16_t address, uint16_t quantity, const uint16_t *registers, uint8_t unit_id, void *arg);
void setup() {
  Serial.begin(115200);
  Ethernet.init(17);
  byte mac[][6] = {{0x30, 0xC6, 0xF7, 0x2F, 0x58, 0xB4}, {0xC0, 0x49, 0xEF, 0xF9, 0xAD, 0x10}, {0x58, 0xBF, 0x25, 0x18, 0xBA, 0x88}};
  Ethernet.begin(mac[idFlow], ip,dns_server,gateway,subnet);
  Serial1.setRX(1); Serial1.setTX(0); Serial1.setFIFOSize(512); Serial1.setTimeout(100); Serial1.begin(9600); while(!Serial1) {}
  mbMaster.begin(9600); mbMaster.setTimeout(100);
  mb = new Modbus(Serial2, idFlow);
  mb->cbVector[CB_READ_DISCRETE_INPUTS] = mb_ReadDiscreteInput;
  mb->cbVector[CB_READ_COILS] = mb_Coil;
  mb->cbVector[CB_WRITE_COILS] = mb_Coil;
  mb->cbVector[CB_READ_HOLDING_REGISTERS] = mb_HoldRegister;
  mb->cbVector[CB_WRITE_HOLDING_REGISTERS] = mb_HoldRegister;
  Serial2.setRX(9); Serial2.setTX(8); Serial2.setFIFOSize(512); Serial2.setTimeout(100); Serial2.begin(9600); while(!Serial2) {}
  mb->begin(9600);
  server.begin();
  nmbs_platform_conf platform_conf;
  platform_conf.transport = NMBS_TRANSPORT_TCP;
  platform_conf.read = read_ethernet;
  platform_conf.write = write_ethernet;
  platform_conf.arg = NULL;    // We will set the arg (socket fd) later
  nmbs_callbacks callbacks = {0};
  callbacks.read_discrete_inputs = handle_read_discrete_inputs;
  callbacks.read_coils = handle_read_coils;
  callbacks.write_single_coil = hadle_write_single_coils;
  callbacks.write_multiple_coils = handle_write_multiple_coils;
  callbacks.read_holding_registers = handler_read_holding_registers;
  callbacks.write_single_register = handler_write_single_register;
  callbacks.write_multiple_registers = handle_write_multiple_registers;
  nmbs_server_create(&nmbs, 1, &platform_conf, &callbacks);
  if (err != NMBS_ERROR_NONE) Serial.println("error create modbus server");
  nmbs_set_read_timeout(&nmbs, 1000);
  for (uint8_t i = 0; i < 6; i++){ btn[i].attach(X[i], INPUT); btn[i].interval(50); btn[i].setPressedState(LOW); pinMode(Y[i], OUTPUT); } pinMode(pin_led, OUTPUT);
  xSemaphore = xSemaphoreCreateMutexStatic(&xMutexBuffer);
  xTaskCreateStatic(TaskslaveId1, "TaskslaveId1", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveid1, &xTaskBuffer_slaveid1);
  xTaskCreateStatic(TaskslaveId2, "TaskslaveId2", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveid2, &xTaskBuffer_slaveid2);
  xTaskCreateStatic(TaskslaveIdWrite, "TaskslaveIdWrite", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveidWrite, &xTaskBuffer_slaveidWrite);
}
void loop() {
  if(IsConnectTCP && millis() - timePrevIsConnectTCP >= 5000) IsConnectTCP = false;
  mb->poll();
  mbTCPpoll();
  vTaskDelay(pdMS_TO_TICKS(1)); // Ganti delay dengan vTaskDelay
}
void TaskslaveId1(void *pvParameters) { (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); read_slave(1); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(100)); } }
void TaskslaveId2(void *pvParameters) { (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); read_slave(2); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(100)); } }
void TaskslaveIdWrite(void *pvParameters) { 
  (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); 
  if(flagWriteMbMasterCoil[0]){ mbMaster.writeSingleCoil(1, startAddresWriteCoil[0], coilBool_Write[0].coil[startAddresWriteCoil[0]]); flagWriteMbMasterCoil[0] = false; }
  if(flagWriteMbMasterCoil[1]){ mbMaster.writeSingleCoil(2, startAddresWriteCoil[1], coilBool_Write[1].coil[startAddresWriteCoil[1]]); flagWriteMbMasterCoil[1] = false; }
  if(flagWriteMbMasterRH[0]){ mbMaster.writeMultipleHoldingRegisters(1, startAddresWriteRH[0], mbFloat_Write[0].words, startAddresWriteRH_length[0]); flagWriteMbMasterRH[0] = false; }
  if(flagWriteMbMasterRH[1]){ mbMaster.writeMultipleHoldingRegisters(2, startAddresWriteRH[1], mbFloat_Write[1].words, startAddresWriteRH_length[1]); flagWriteMbMasterRH[1] = false; }
  xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(1)); } 
}
void read_slave(uint8_t id){
  if(countStepMbmaster[id-1] ++ > 3) countStepMbmaster[id-1] = 0;
  switch (countStepMbmaster[id-1]){
  case 1: mbMaster.readHoldingRegisters(id, 0, mbFloat[id-1].words, 15); break;
  case 2: mbMaster.readCoils(id, 0, coilBool[id-1].coil, 5); break;
  case 3: mbMaster.readDiscreteInputs(id, 0, coilBool[id-1].inputDiscrete, 2); break; }
  digitalWrite(pin_led, stateLed = !stateLed);
}
uint8_t mb_ReadDiscreteInput(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb_sizeDiscreateInput || (address + length) > mb_sizeDiscreateInput) return STATUS_ILLEGAL_DATA_ADDRESS;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(index < 2) mb->writeDiscreteInputToBuffer(i, coilBool[0].inputDiscrete[index]);
    else if(index >= 2 && index < 4) mb->writeDiscreteInputToBuffer(i, coilBool[1].inputDiscrete[index - 2]);
    else if(index >= 4 && index < 10) mb->writeDiscreteInputToBuffer(i, !digitalRead(X[index - 4]));
    else mb->writeDiscreteInputToBuffer(i, IsConnectTCP);
  }
  return STATUS_OK;
}
uint8_t mb_Coil(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb_sizeCoil || (address + length) > mb_sizeCoil) return STATUS_ILLEGAL_DATA_ADDRESS;
  bool flagWrite1 = false, flagWrite2 = false;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(fc == FC_READ_COILS){
      if(index < 5) mb->writeCoilToBuffer(i, coilBool[0].coil[index]);
      else if(index >= 5 && index < 10) mb->writeCoilToBuffer(i, coilBool[1].coil[index - 5]);
      else if(index < 16 && index >= 10) mb->writeCoilToBuffer(i, digitalRead(Y[index - 10]));
      else mb->writeCoilToBuffer(i, mb_flagCoil[index - 16]);
    }else{
      if(index < 5) { 
        coilBool_Write[0].coil[index] = mb->readCoilFromBuffer(i); startAddresWriteCoil[0] = index;
        flagWrite1 = true; 
      }
      else if(index >= 5 && index < 10) { 
        coilBool_Write[1].coil[index - 5] = mb->readCoilFromBuffer(i); startAddresWriteCoil[1] = index - 5;
        flagWrite2 = true; 
      }
      else if(index < 16 && index >= 10) digitalWrite(Y[index - 10], mb->readCoilFromBuffer(i));
      else mb_flagCoil[index - 16] = mb->readCoilFromBuffer(i);
    }
  }
  if(flagWrite1) { flagWriteMbMasterCoil[0] = true; }
  if(flagWrite2) { flagWriteMbMasterCoil[1] = true; }
  return STATUS_OK;
}
uint8_t mb_HoldRegister(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb_sizeHoldingRegister || (address + length) > mb_sizeHoldingRegister) return STATUS_ILLEGAL_DATA_ADDRESS;
  bool flagWrite1 = false, flagWrite2 = false;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(fc == FC_READ_HOLDING_REGISTERS){
      if(index < 15) mb->writeRegisterToBuffer(i, mbFloat[0].words[index]);
      else if(index >= 15 && index < 30) mb->writeRegisterToBuffer(i, mbFloat[1].words[index - 15]);
    }else{
      if(index < 15) { 
        mbFloat_Write[0].words[i] = mb->readRegisterFromBuffer(i); startAddresWriteRH[0] = index >= 12 ? index : index - 1; startAddresWriteRH_length[0] = length;
        flagWrite1 = true; 
      }
      else if(index >= 15 && index < 30){ 
        mbFloat_Write[1].words[i] = mb->readRegisterFromBuffer(i); startAddresWriteRH[1] = (index - 15) >= 12 ? (index - 15) : (index - 16); startAddresWriteRH_length[1] = length;
        flagWrite2 = true; 
      }
    }
  }
  if(flagWrite1) {flagWriteMbMasterRH[0] = true;}
  if(flagWrite2) {flagWriteMbMasterRH[1] = true;}
  return STATUS_OK;
}
nmbs_error handle_read_discrete_inputs(uint16_t address, uint16_t quantity, uint8_t *inputs_out, uint8_t unit_id, void *arg){
  Serial.println("ok");
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeDiscreateInput + 1) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++) {
        uint8_t index = address + i;
        if(index < 2) nmbs_bitfield_write(inputs_out, i, coilBool[0].inputDiscrete[index]);
        else if(index >= 2 && index < 4)nmbs_bitfield_write(inputs_out, i, coilBool[1].inputDiscrete[index - 2]);
        else if(index >= 4 && index < 10) nmbs_bitfield_write(inputs_out, i, !digitalRead(X[index - 4]));
        else nmbs_bitfield_write(inputs_out, i, IsConnectTCP);
    }
    return NMBS_ERROR_NONE;
}
nmbs_error handle_read_coils(uint16_t address, uint16_t quantity, nmbs_bitfield coils_out, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeCoil + 1) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++) {
      uint8_t index = address + i;
      if(index < 5)nmbs_bitfield_write(coils_out, i, coilBool[0].coil[index]);
      else if(index >= 5 && index < 10) nmbs_bitfield_write(coils_out, i, coilBool[1].coil[index - 5]);
      else if(index < 16 && index >= 10) nmbs_bitfield_write(coils_out, i, digitalRead(Y[index - 10]));
      else nmbs_bitfield_write(coils_out, i, mb_flagCoil[index - 16]);
    }
    return NMBS_ERROR_NONE;
}
nmbs_error hadle_write_single_coils(uint16_t address, bool value, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address > mb_sizeCoil) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    bool flagWrite1 = false, flagWrite2 = false;
    for (int i = 0; i < mb_sizeCoil; i++){
      if(address < 5 && address == i) { 
        coilBool_Write[0].coil[address] = value; startAddresWriteCoil[0] = address;
        flagWrite1 = true; 
      }
      else if(address >= 5 && address < 10 && address == i) { 
        coilBool_Write[1].coil[address - 5] = value; startAddresWriteCoil[1] = address - 5;
        flagWrite2 = true; 
      }
      else if(address < 16 && address >= 10 && address == i) digitalWrite(Y[address - 10], value);
      else if( address >= 16 && address == i) mb_flagCoil[address - 16] = value;
    }
    if(flagWrite1) { flagWriteMbMasterCoil[0] = true; }
    if(flagWrite2) { flagWriteMbMasterCoil[1] = true; }
    return NMBS_ERROR_NONE;
}

nmbs_error handle_write_multiple_coils(uint16_t address, uint16_t quantity, const uint8_t *coils, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeCoil + 1) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    bool flagWrite1 = false, flagWrite2 = false;
    for (int i = 0; i < quantity; i++){
      uint8_t index = address + i;
      if(address < 5) { 
        nmbs_bitfield_write(coilBool_Write[0].coil, index, coils[i]); startAddresWriteCoil[0] = address;
        flagWrite1 = true; 
      }
      else if(address >= 5 && address < 10) { 
        nmbs_bitfield_write(coilBool_Write[1].coil, index - 5, coils[i]); startAddresWriteCoil[1] = address - 5;
        flagWrite2 = true; 
      }
      else if(address < 16 && address >= 10) digitalWrite(Y[index - 10], coils[i]);
      else nmbs_bitfield_write(mb_flagCoil, index - 16, nmbs_bitfield_read(coils, i));
    }
    if(flagWrite1) { flagWriteMbMasterCoil[0] = true; }
    if(flagWrite2) { flagWriteMbMasterCoil[1] = true; }
    return NMBS_ERROR_NONE;
}
nmbs_error handler_read_holding_registers(uint16_t address, uint16_t quantity, uint16_t *registers_out, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++){
      uint8_t index = address + i;
      if(index < 15) registers_out[i] = mbFloat[0].words[index];
      else if(index >= 15 && index < 30) registers_out[i] = mbFloat[1].words[index - 15];
    }
   return NMBS_ERROR_NONE;
}
nmbs_error handler_write_single_register(uint16_t address, uint16_t value, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    bool flagWrite1 = false, flagWrite2 = false;
    for (int i = 0; i < mb_sizeHoldingRegister; i++){
      if(address < 15 && address == i) { 
        mbFloat_Write[0].words[i] = value; startAddresWriteRH[0] = address; startAddresWriteRH_length[0] = 1;
        flagWrite1 = true; 
      }
      else if(address >= 15 && address < 30 && address == i){ 
        mbFloat_Write[1].words[i] = value; startAddresWriteRH[1] = address - 15; startAddresWriteRH_length[1] = 1;
        flagWrite2 = true; 
      }
    }
    if(flagWrite1) {flagWriteMbMasterRH[0] = true;}
    if(flagWrite2) {flagWriteMbMasterRH[1] = true;}
    return NMBS_ERROR_NONE;
}
nmbs_error handle_write_multiple_registers(uint16_t address, uint16_t quantity, const uint16_t *registers, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    bool flagWrite1 = false, flagWrite2 = false;
    for (int i = 0; i < quantity; i++){
      int8_t index = address + i;
      if(index < 15) { 
        mbFloat_Write[0].words[i] = registers[i]; startAddresWriteRH[0] = index >= 12 ? index : index - 1; startAddresWriteRH_length[0] = quantity;
        flagWrite1 = true; 
      }
      else if(index >= 15 && index < 30){ 
        mbFloat_Write[1].words[i] = registers[i]; startAddresWriteRH[1] = (index - 15) >= 12 ? (index - 15) : (index - 16); startAddresWriteRH_length[1] = quantity;
        flagWrite2 = true; 
      }
    }
    if(flagWrite1) {flagWriteMbMasterRH[0] = true;}
    if(flagWrite2) {flagWriteMbMasterRH[1] = true;}
    return NMBS_ERROR_NONE;
}
int32_t read_ethernet(uint8_t* buf, uint16_t count, int32_t timeout_ms, void* arg) {
  uint16_t total = 0;
  unsigned long startMillis = millis();
  while (total != count) {
    buf[total++] = client.read();
    if (millis() - startMillis > timeout_ms) {
      return total;
    }
  }
  return total;
}

int32_t write_ethernet(const uint8_t* buf, uint16_t count, int32_t timeout_ms, void* arg) {
  uint16_t total = 0;
  unsigned long startMillis = millis();
  while (total != count) {
    client.write(buf[total++]);
    if (millis() - startMillis > timeout_ms) {
      return total;
    }
  }
  return total;
}
void mbTCPpoll(){
  client = server.available();
  if (client){
    if (client.connected() && client.available()){
      err = nmbs_server_poll(&nmbs);
      timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
    }
  }
}
