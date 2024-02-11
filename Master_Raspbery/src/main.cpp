#include <Arduino.h>
#include <stdio.h>
#include <cstring>
#include <FreeRTOS.h>
#include <task.h>
#include <semphr.h>
#include <Bounce2.h>
#include <SPI.h>
#include <Ethernet2.h>
#include <nanomodbus.h>
#define UNUSED_PARAM(x) ((void)(x))

//uint8_t idFlow = 1; IPAddress ip(192, 168, 1, 123);
uint8_t idFlow = 1; IPAddress ip(192, 168, 1, 124);
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
uint8_t startAddresWriteCoil[MaxId], startAddresWriteCoil_length[MaxId], startAddresWriteRH[MaxId], startAddresWriteRH_length[MaxId];

unsigned long timePrevIsConnectTCP;
bool stateLed, IsConnectTCP, mb_flagCoil[20];
uint8_t mb_sizeDiscreateInput = ((sizeof(coilBool[0].inputDiscrete) / sizeof(coilBool[0].inputDiscrete[0])) * MaxId) + (sizeof(X) / sizeof(X[0]) + 1);
uint8_t mb_sizeCoil = ((sizeof(coilBool[0].coil) / sizeof(coilBool[0].coil[0])) * MaxId) + (sizeof(Y) / sizeof(Y[0])) + (sizeof(mb_flagCoil) / sizeof(mb_flagCoil[0]));
uint8_t mb_sizeHoldingRegister = ((sizeof(mbFloat[0].words) / sizeof(mbFloat[0].words[0])) * MaxId);

EthernetServer server(502);
EthernetClient client;
nmbs_t nmbsTCP, nmbsRTU, nmbsClient[MaxId]; nmbs_error errTCP, errRTU, errClient[MaxId];
Bounce2::Button btn[6];
StaticTask_t xTaskBuffer_slaveid1, xTaskBuffer_slaveid2, xTaskBuffer_slaveidWrite;
StackType_t xStack_slaveid1[400], xStack_slaveid2[400], xStack_slaveidWrite[400];
SemaphoreHandle_t xSemaphore = NULL;
StaticSemaphore_t xMutexBuffer;

void TaskslaveId1(void *pvParameters), TaskslaveId2(void *pvParameters), TaskslaveIdWrite(void *pvParameters);
void write_slave(uint8_t id);
void read_slave(uint8_t id);
int32_t read_ethernet(uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg);
int32_t write_ethernet(const uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg);
void mbTCPpoll();
int32_t read_serial2(uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg);
int32_t write_serial2(const uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg);
void mbRTUpoll();
int32_t read_serial1(uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg);
int32_t write_serial1(const uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg);
nmbs_error handle_read_discrete_inputs(uint16_t address, uint16_t quantity, uint8_t *inputs_out, uint8_t unit_id, void *arg);
nmbs_error handle_read_coils(uint16_t address, uint16_t quantity, nmbs_bitfield coils_out, uint8_t unit_id, void *arg);
nmbs_error hadle_write_single_coils(uint16_t address, bool value, uint8_t unit_id, void *arg);
nmbs_error handle_write_multiple_coils(uint16_t address, uint16_t quantity, const uint8_t *coils, uint8_t unit_id, void *arg);
nmbs_error handler_read_holding_registers(uint16_t address, uint16_t quantity, uint16_t *registers_out, uint8_t unit_id, void *arg);
nmbs_error handler_write_single_register(uint16_t address, uint16_t value, uint8_t unit_id, void *arg);
nmbs_error handle_write_multiple_registers(uint16_t address, uint16_t quantity, const uint16_t *registers, uint8_t unit_id, void *arg);
void setup() {
  Serial.begin(115200);
  Serial1.setRX(1); Serial1.setTX(0); Serial1.setFIFOSize(512); Serial1.setTimeout(100); Serial1.begin(9600); while(!Serial1) {}
  nmbs_platform_conf platform_confClient;
  platform_confClient.transport = NMBS_TRANSPORT_RTU;
  platform_confClient.read = read_serial1;
  platform_confClient.write = write_serial1;
  Ethernet.init(17);
  byte mac[][6] = {{0x30, 0xC6, 0xF7, 0x2F, 0x58, 0xB4}, {0xC0, 0x49, 0xEF, 0xF9, 0xAD, 0x10}, {0x58, 0xBF, 0x25, 0x18, 0xBA, 0x88}};
  Ethernet.begin(mac[idFlow], ip,dns_server,gateway,subnet);
  server.begin();
  nmbs_platform_conf platform_confTCP;
  platform_confTCP.transport = NMBS_TRANSPORT_TCP;
  platform_confTCP.read = read_ethernet;
  platform_confTCP.write = write_ethernet;
  platform_confTCP.arg = NULL;    // We will set the arg (socket fd) later
  Serial2.setRX(9); Serial2.setTX(8); Serial2.setFIFOSize(512); Serial2.setTimeout(100); Serial2.begin(9600); while(!Serial2) {}
  nmbs_platform_conf platform_confRTU;
  platform_confRTU.transport = NMBS_TRANSPORT_RTU;
  platform_confRTU.read = read_serial2;
  platform_confRTU.write = write_serial2;
  platform_confRTU.arg = NULL;    // We will set the arg (socket fd) later
  nmbs_callbacks callbacks = {0};
  callbacks.read_discrete_inputs = handle_read_discrete_inputs;
  callbacks.read_coils = handle_read_coils;
  callbacks.write_single_coil = hadle_write_single_coils;
  callbacks.write_multiple_coils = handle_write_multiple_coils;
  callbacks.read_holding_registers = handler_read_holding_registers;
  callbacks.write_single_register = handler_write_single_register;
  callbacks.write_multiple_registers = handle_write_multiple_registers;
  errClient[0] = nmbs_client_create(&nmbsClient[0], &platform_confClient);
  if (errClient[0] != NMBS_ERROR_NONE) Serial.printf("Error on modbus connection RTU Client 1 - %s\n", nmbs_strerror(errClient[0]));
  nmbs_set_read_timeout(&nmbsClient[0], 200);
  nmbs_set_byte_timeout(&nmbsClient[0], 100);
  nmbs_set_destination_rtu_address(&nmbsClient[0], 1); // address slave
  errClient[1] = nmbs_client_create(&nmbsClient[1], &platform_confClient);
  if (errClient[1] != NMBS_ERROR_NONE) Serial.printf("Error on modbus connection RTU Client 2 - %s\n", nmbs_strerror(errClient[1]));
  nmbs_set_read_timeout(&nmbsClient[1], 200);
  nmbs_set_byte_timeout(&nmbsClient[1], 100);
  nmbs_set_destination_rtu_address(&nmbsClient[1], 2); // address slave
  errTCP = nmbs_server_create(&nmbsTCP, 1, &platform_confTCP, &callbacks);
  if (errTCP != NMBS_ERROR_NONE) Serial.printf("Error on modbus connection TCP - %s\n", nmbs_strerror(errTCP));
  nmbs_set_read_timeout(&nmbsTCP, 1000);
  errRTU = nmbs_server_create(&nmbsRTU, idFlow, &platform_confRTU, &callbacks);
  if (errRTU != NMBS_ERROR_NONE) Serial.printf("Error on modbus connection RTU Server - %s\n", nmbs_strerror(errRTU));
  nmbs_set_read_timeout(&nmbsRTU, 1000);
  nmbs_set_byte_timeout(&nmbsRTU, 100);
  for (uint8_t i = 0; i < 6; i++){ btn[i].attach(X[i], INPUT); btn[i].interval(50); btn[i].setPressedState(LOW); pinMode(Y[i], OUTPUT); } pinMode(pin_led, OUTPUT);
  xSemaphore = xSemaphoreCreateMutexStatic(&xMutexBuffer);
  xTaskCreateStatic(TaskslaveId1, "TaskslaveId1", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveid1, &xTaskBuffer_slaveid1);
  xTaskCreateStatic(TaskslaveId2, "TaskslaveId2", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveid2, &xTaskBuffer_slaveid2);
  xTaskCreateStatic(TaskslaveIdWrite, "TaskslaveIdWrite", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveidWrite, &xTaskBuffer_slaveidWrite);
}
void loop() {
  if(IsConnectTCP && millis() - timePrevIsConnectTCP >= 5000) IsConnectTCP = false;
  mbRTUpoll();
  mbTCPpoll();
  vTaskDelay(pdMS_TO_TICKS(1)); // Ganti delay dengan vTaskDelay
}
void TaskslaveId1(void *pvParameters) { (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); read_slave(0); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(100)); } }
void TaskslaveId2(void *pvParameters) { (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); read_slave(1); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(100)); } }
void TaskslaveIdWrite(void *pvParameters) {  (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY);  write_slave(0); write_slave(1); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(1)); }  }
void write_slave(uint8_t id){
  if(flagWriteMbMasterCoil[id]){
    if(startAddresWriteCoil_length[id] > 1){
      nmbs_bitfield coilsBuffer = {0};
      for (int i = 0; i < startAddresWriteCoil_length[id]; i++) nmbs_bitfield_write(coilsBuffer, i, coilBool_Write[id].coil[startAddresWriteCoil[id] + i]);
      errClient[id] = nmbs_write_multiple_coils(&nmbsClient[id], startAddresWriteCoil[id], startAddresWriteCoil_length[id], coilsBuffer);
    }else{
      errClient[id] = nmbs_write_single_coil(&nmbsClient[id], startAddresWriteCoil[id], coilBool_Write[id].coil[startAddresWriteCoil[id]]);
    }
    flagWriteMbMasterCoil[id] = false; 
  }
  if(flagWriteMbMasterRH[id]){
    if(startAddresWriteRH_length[id] > 1){
      uint16_t w_regs[100];
      for (int i = 0; i < startAddresWriteRH_length[id]; i++) w_regs[i] = mbFloat_Write[id].words[startAddresWriteRH[id] + i];
      
      errClient[id] = nmbs_write_multiple_registers(&nmbsClient[id], startAddresWriteRH[id], startAddresWriteRH_length[id], w_regs);
    }else{
      errClient[id] = nmbs_write_single_register(&nmbsClient[id], startAddresWriteRH[id], mbFloat_Write[id].words[startAddresWriteRH[id]]);
    }
    flagWriteMbMasterRH[id] = false; 
  }
}
void read_slave(uint8_t id){
  switch (countStepMbmaster[id]){
  case 0:{
      mbFloatInt mbFloatBuffer;
      errClient[id] = nmbs_read_holding_registers(&nmbsClient[id], 0, 15, mbFloatBuffer.words);
      if(errClient[id] == NMBS_ERROR_NONE) memcpy(&mbFloat[id], &mbFloatBuffer, sizeof(mbFloatInt));
    }
    break;
  case 1:{
      nmbs_bitfield coils_Buffer = {0};
      errClient[id] = nmbs_read_coils(&nmbsClient[id], 0, 5, coils_Buffer);
      if(errClient[id] == NMBS_ERROR_NONE)
      { for (int i = 0; i < 5; i++) {  coilBool[id].coil[i] = nmbs_bitfield_read(coils_Buffer, i); }} 
    }
    break;
  case 2:{
      nmbs_bitfield disBuffer = {0};
      errClient[id] = nmbs_read_discrete_inputs(&nmbsClient[id], 0, 2, disBuffer);
      if(errClient[id] == NMBS_ERROR_NONE) { for (int i = 0; i < 2; i++) { coilBool[id].inputDiscrete[i] = nmbs_bitfield_read(disBuffer, i); }}
    }
    break;
  }
  digitalWrite(pin_led, stateLed = !stateLed);
  if(countStepMbmaster[id] ++ >= 2) countStepMbmaster[id] = 0;
}
nmbs_error handle_read_discrete_inputs(uint16_t address, uint16_t quantity, uint8_t *inputs_out, uint8_t unit_id, void *arg){
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
      if(index < 5) nmbs_bitfield_write(coils_out, i, coilBool[0].coil[index]);
      else if(index >= 5 && index < 10) nmbs_bitfield_write(coils_out, i, coilBool[1].coil[index - 5]);
      else if(index < 16 && index >= 10) nmbs_bitfield_write(coils_out, i, digitalRead(Y[index - 10]));
      else nmbs_bitfield_write(coils_out, i, mb_flagCoil[index - 16]);
    }
    return NMBS_ERROR_NONE;
}
nmbs_error hadle_write_single_coils(uint16_t address, bool value, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address > mb_sizeCoil) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    if(address < 5){
      coilBool_Write[0].coil[address] = value; startAddresWriteCoil[0] = address; startAddresWriteCoil_length[0] = 1; flagWriteMbMasterCoil[0] = true;
    }else if(address >= 5 && address < 10){
      coilBool_Write[1].coil[address - 5] = value; startAddresWriteCoil[1] = address - 5; startAddresWriteCoil_length[1] = 1; flagWriteMbMasterCoil[1] = true;
    }else if(address < 16 && address >= 10) { digitalWrite(Y[address - 10], value);}
    else if( address >= 16) mb_flagCoil[address - 16] = value;
    return NMBS_ERROR_NONE;
}
nmbs_error handle_write_multiple_coils(uint16_t address, uint16_t quantity, const uint8_t *coils, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeCoil + 1) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    bool flagWrite1 = false, flagWrite2 = false; uint16_t _quantity[2];
    for (int i = 0; i < quantity; i++){
      uint8_t index = address + i;
      if(index < 5) { coilBool_Write[0].coil[index] = nmbs_bitfield_read(coils, i); _quantity[0]++; flagWrite1 = true; }
      else if(index >= 5 && index < 10) { coilBool_Write[1].coil[index - 5] = nmbs_bitfield_read(coils, i); _quantity[1]++; flagWrite2 = true; }
      else if(index < 16 && index >= 10) digitalWrite(Y[index - 10], nmbs_bitfield_read(coils, i));
      else mb_flagCoil[index - 16] = nmbs_bitfield_read(coils, i);
    }
    if(flagWrite1) { startAddresWriteCoil[0] = address; startAddresWriteCoil_length[0] = _quantity[0]; flagWriteMbMasterCoil[0] = true; }
    if(flagWrite2) { startAddresWriteCoil[1] = address < 5 ? ((address + quantity) - 5) - _quantity[1]: address - 5; startAddresWriteCoil_length[1] = _quantity[1]; flagWriteMbMasterCoil[1] = true; }
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
    if(address < 15){
      mbFloat_Write[0].words[address] = value; startAddresWriteRH[0] = address; startAddresWriteRH_length[0] = 1; flagWriteMbMasterRH[0] = true;
    }else if(address >= 15){
      mbFloat_Write[1].words[address - 15] = value; startAddresWriteRH[1] = address - 15; startAddresWriteRH_length[1] = 1; flagWriteMbMasterRH[1] = true;
    }
    return NMBS_ERROR_NONE;
}
nmbs_error handle_write_multiple_registers(uint16_t address, uint16_t quantity, const uint16_t *registers, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    bool flagWrite1 = false, flagWrite2 = false; uint16_t _quantity[2];
    for (int i = 0; i < quantity; i++){
      int8_t index = address + i;
      if(index < 15) { 
        mbFloat_Write[0].words[index] = registers[i]; _quantity[0]++;
        flagWrite1 = true; 
      }
      else if(index >= 15 && index < 30){ 
        mbFloat_Write[1].words[index - 15] = registers[i]; _quantity[1]++;
        flagWrite2 = true;
      }
    }
    if(flagWrite1) { startAddresWriteRH[0] = address; startAddresWriteRH_length[0] = _quantity[0];  flagWriteMbMasterRH[0] = true;}
    if(flagWrite2) { startAddresWriteRH[1] = address < 15 ? ((address + quantity) - 15) - _quantity[1] : address - 15; startAddresWriteRH_length[1] = _quantity[1]; flagWriteMbMasterRH[1] = true;}
    return NMBS_ERROR_NONE;
}
int32_t read_ethernet(uint8_t* buf, uint16_t count, int32_t timeout_ms, void* arg) {
  client.setTimeout(timeout_ms); return client.readBytes(buf, count);
}
int32_t write_ethernet(const uint8_t* buf, uint16_t count, int32_t timeout_ms, void* arg) {
  client.setTimeout(timeout_ms); return client.write(buf, count);
}
void mbTCPpoll(){
  client = server.available();
  if (client.connected() && client.available()){
    errTCP = nmbs_server_poll(&nmbsTCP);
    timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
    if (errTCP != NMBS_ERROR_NONE) Serial.printf("Error on modbus TCP - %s\n", nmbs_strerror(errTCP));
    else client.flush();
  }
}
int32_t read_serial2(uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg) {
  Serial2.setTimeout(byte_timeout_ms); return Serial2.readBytes(buf, count);
}
int32_t write_serial2(const uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg) {
  Serial2.setTimeout(byte_timeout_ms); return Serial2.write(buf, count);
}
void mbRTUpoll(){
  if(Serial2.available() > 0) {
    errRTU = nmbs_server_poll(&nmbsRTU);
    if (errTCP != NMBS_ERROR_NONE) Serial.printf("Error on modbus RTU - %s\n", nmbs_strerror(errTCP));
    else Serial2.flush();
  }
}
int32_t read_serial1(uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg) {
  Serial1.setTimeout(byte_timeout_ms);
  return Serial1.readBytes(buf, count);
}
int32_t write_serial1(const uint8_t* buf, uint16_t count, int32_t byte_timeout_ms, void* arg) {
  Serial1.setTimeout(byte_timeout_ms);
  return Serial1.write(buf, count);
}