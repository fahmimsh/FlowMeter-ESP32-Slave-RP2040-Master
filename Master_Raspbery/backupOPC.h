#include <Arduino.h>
#include <stdio.h>
#include <FreeRTOS.h>
#include <task.h>
#include <semphr.h>
#include <ModbusSlave.h>
#include <ModbusRTUMaster.h>
#include <Bounce2.h>
#include <Ethernet.h>
#include <OPC.h>
#include <Bridge.h>
#include <Ethernet.h>
#include <SPI.h>
#include <OPC_Defines.h>


//uint8_t idFlow1 = 1, idFlow2 = 2, idOPC_RP2040 = 0; IPAddress ip(192, 168, 1, 123);
uint8_t idFlow1 = 3, idFlow2 = 4, idOPC_RP2040 = 1; IPAddress ip(192, 168, 1, 124);
IPAddress gateway(192, 168, 1, 1), dns_server(192, 168, 110, 201), subnet(255,255,255,0);
#define STACK_SIZE 200
pin_size_t X[6] = {12, 13, 14, 15, 21, 22};
pin_size_t Y[6] = {2, 3, 6, 7, 10, 11};
pin_size_t pin_led = 25;
const int MaxId = 2;
union mbFloatInt{
  struct {
    float kFact; float capacity; float setPoint; float factorKurang; float liter; float sec;  float freq; float Correct; float Flowrate; float Volume;
    uint16_t slave_id; uint16_t baudrate; uint16_t timeInterval_OnValve; uint16_t timeInterval_OffValve; uint16_t over_fl_err;
  } values;
  uint16_t words[25];
};
mbFloatInt mbFloat[MaxId], mbFloat_Write[MaxId];;
struct CoilBool { bool coil[10]; bool inputDiscrete[2]; };
CoilBool coilBool[MaxId], coilBool_Write[MaxId];
uint8_t countStepMbmaster[MaxId];
bool mbFlagReadSlaveid[MaxId];

unsigned long timePrevIsConnectTCP;
bool stateLed, IsConnectTCP, mb_flagCoil[14];
uint16_t mb_holdRegister[5];
bool mb1_write_coil, mb2_write_coil, mb1_write_rhd, mb2_write_rhd;
uint8_t mb1_write_coil_startAddress, mb2_write_coil_startAddress, mb1_write_coil_length, mb2_write_coil_length;
uint8_t mb1_write_rhd_startAddress, mb2_write_rhd_startAddress, mb1_write_rhd_length, mb2_write_rhd_length;
//ADD FLAG CON TCP, poll mbslave -> id 0-9
uint8_t mb1_sizeDiscreateInput = (sizeof(coilBool[0].inputDiscrete) / sizeof(coilBool[0].inputDiscrete[0])) + (sizeof(X) / sizeof(X[0]) + 2);
uint8_t mb2_sizeDiscreateInput = (sizeof(coilBool[1].inputDiscrete) / sizeof(coilBool[1].inputDiscrete[0])) + (sizeof(X) / sizeof(X[0]) + 2);
//id 0-29
uint8_t mb1_sizeCoil = (sizeof(coilBool[0].coil) / sizeof(coilBool[0].coil[0])) + (sizeof(Y) / sizeof(Y[0])) + (sizeof(mb_flagCoil) / sizeof(mb_flagCoil[0]));
uint8_t mb2_sizeCoil = (sizeof(coilBool[1].coil) / sizeof(coilBool[1].coil[0])) + (sizeof(Y) / sizeof(Y[0])) + (sizeof(mb_flagCoil) / sizeof(mb_flagCoil[0]));
//id 0-29
uint8_t mb1_sizeHoldingRegister = (sizeof(mbFloat[0].words) / sizeof(mbFloat[0].words[0])) + (sizeof(mb_holdRegister) / sizeof(mb_holdRegister[0]));
uint8_t mb2_sizeHoldingRegister = (sizeof(mbFloat[1].words) / sizeof(mbFloat[1].words[0])) + (sizeof(mb_holdRegister) / sizeof(mb_holdRegister[0]));

OPCEthernet opc;
Bounce2::Button btn[6];
ModbusRTUMaster mbMaster(Serial1);
ModbusSlave mb[2] = {ModbusSlave(idFlow1), ModbusSlave(idFlow2)};
Modbus mbs(Serial2, mb, 2);
StaticTask_t xTaskBuffer_slaveid1, xTaskBuffer_slaveid2, xTaskBuffer_slaveid1_write_coil, xTaskBuffer_slaveid2_write_coil, xTaskBuffer_slaveid1_write_rhd, xTaskBuffer_slaveid2_write_rhd;
StackType_t xStack_slaveid1[400], xStack_slaveid2[400], xStack_slaveid1_write_coil[200], xStack_slaveid2_write_coil[200], xStack_slaveid1_write_rhd[200], xStack_slaveid2_write_rhd[200];
SemaphoreHandle_t xSemaphore = NULL;
StaticSemaphore_t xMutexBuffer;

void TaskslaveId1(void *pvParameters), TaskslaveId2(void *pvParameters);
void TaskslaveId1_write_coil(void *pvParameters), TaskslaveId2_write_coil(void *pvParameters), TaskslaveId1_write_rhd(void *pvParameters), TaskslaveId2_write_rhd(void *pvParameters);
void read_slave(uint8_t id);
uint8_t mb1_ReadDiscreteInput(uint8_t fc, uint16_t address, uint16_t length);
uint8_t mb1_Coil(uint8_t fc, uint16_t address, uint16_t length);
uint8_t mb1_HoldRegister(uint8_t fc, uint16_t address, uint16_t length);
uint8_t mb2_ReadDiscreteInput(uint8_t fc, uint16_t address, uint16_t length);
uint8_t mb2_Coil(uint8_t fc, uint16_t address, uint16_t length);
uint8_t mb2_HoldRegister(uint8_t fc, uint16_t address, uint16_t length);
bool itemDisCreated(const char *itemID, const opcOperation opcOP, const bool value);
bool itemDisCoil(const char *itemID, const opcOperation opcOP, const bool value);
int itemInt(const char *itemID, const opcOperation opcOP, const int value);
float itemFloat(const char *itemID, const opcOperation opcOP, const float value);
void setup() {
  Serial.begin(115200);
  Serial1.setRX(1); Serial1.setTX(0); Serial1.setFIFOSize(512); Serial1.setTimeout(100); Serial1.begin(9600); while(!Serial1) {}
  mbMaster.begin(9600); mbMaster.setTimeout(100);
  mb[0].cbVector[CB_READ_DISCRETE_INPUTS] = mb1_ReadDiscreteInput;
  mb[0].cbVector[CB_READ_COILS] = mb1_Coil;
  mb[0].cbVector[CB_WRITE_COILS] = mb1_Coil;
  mb[0].cbVector[CB_READ_HOLDING_REGISTERS] = mb1_HoldRegister;
  mb[0].cbVector[CB_WRITE_HOLDING_REGISTERS] = mb1_HoldRegister;
  mb[1].cbVector[CB_READ_DISCRETE_INPUTS] = mb2_ReadDiscreteInput;
  mb[1].cbVector[CB_READ_COILS] = mb2_Coil;
  mb[1].cbVector[CB_WRITE_COILS] = mb2_Coil;
  mb[1].cbVector[CB_READ_HOLDING_REGISTERS] = mb2_HoldRegister;
  mb[1].cbVector[CB_WRITE_HOLDING_REGISTERS] = mb2_HoldRegister;
  Serial2.setRX(9); Serial2.setTX(8); Serial2.setFIFOSize(512); Serial2.setTimeout(100); Serial2.begin(9600); while(!Serial2) {}
  mbs.begin(9600);
  for (uint8_t i = 0; i < 6; i++){ btn[i].attach(X[i], INPUT); btn[i].interval(50); btn[i].setPressedState(LOW); pinMode(Y[i], OUTPUT); } pinMode(pin_led, OUTPUT);
  opc.setup(80, idOPC_RP2040, ip, dns_server, gateway, subnet);
  for (uint8_t i = 0; i < size_item_D; i++) opc.addItem(ItemD[i], opc_read, opc_bool, itemDisCreated);
  for (uint8_t i = 0; i < size_item_C; i++) opc.addItem(ItemC[i], opc_readwrite, opc_bool, itemDisCoil);
  for (uint8_t i = 0; i < size_item_I; i++) opc.addItem(ItemI[i], opc_readwrite, opc_int, itemInt);
  for (uint8_t i = 0; i < size_item_F; i++) opc.addItem(ItemF[i], opc_readwrite, opc_float, itemFloat);
  xSemaphore = xSemaphoreCreateMutexStatic(&xMutexBuffer);
  xTaskCreateStatic(TaskslaveId1, "TaskslaveId1", 400, NULL, configMAX_PRIORITIES - 2, xStack_slaveid1, &xTaskBuffer_slaveid1);
  xTaskCreateStatic(TaskslaveId2, "TaskslaveId2", 400, NULL, configMAX_PRIORITIES - 2, xStack_slaveid2, &xTaskBuffer_slaveid2);
  xTaskCreateStatic(TaskslaveId1_write_coil, "TaskslaveId1_write_coil", 200, NULL, configMAX_PRIORITIES - 1, xStack_slaveid1_write_coil, &xTaskBuffer_slaveid1_write_coil);
  xTaskCreateStatic(TaskslaveId2_write_coil, "TaskslaveId2_write_coil", 200, NULL, configMAX_PRIORITIES - 1, xStack_slaveid2_write_coil, &xTaskBuffer_slaveid2_write_coil);
  xTaskCreateStatic(TaskslaveId1_write_rhd, "TaskslaveId1_write_rhd", 200, NULL, configMAX_PRIORITIES - 1, xStack_slaveid1_write_rhd, &xTaskBuffer_slaveid1_write_rhd);
  xTaskCreateStatic(TaskslaveId2_write_rhd, "TaskslaveId2_write_rhd", 200, NULL, configMAX_PRIORITIES - 1, xStack_slaveid2_write_rhd, &xTaskBuffer_slaveid2_write_rhd);
}
void loop() {
  if(IsConnectTCP && millis() - timePrevIsConnectTCP >= 5000) IsConnectTCP = false;
  mbs.poll();
  opc.processOPCCommands();
  vTaskDelay(pdMS_TO_TICKS(1)); // Ganti delay dengan vTaskDelay
}
void TaskslaveId1(void *pvParameters) { (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); read_slave(1); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(200)); } }
void TaskslaveId2(void *pvParameters) { (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); read_slave(2); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(200)); } }
void TaskslaveId1_write_coil(void *pvParameters){ 
  (void)pvParameters; while (1) {
    xSemaphoreTake(xSemaphore, portMAX_DELAY); 
    if(mb1_write_coil){ mbMaster.writeMultipleCoils(1, mb1_write_coil_startAddress, coilBool_Write[0].coil, mb1_write_coil_length); mb1_write_coil = false; }
    xSemaphoreGive(xSemaphore);
  vTaskDelay(pdMS_TO_TICKS(1));
  }
} 
void TaskslaveId2_write_coil(void *pvParameters){
  (void)pvParameters; while (1) {
    xSemaphoreTake(xSemaphore, portMAX_DELAY);
    if(mb2_write_coil){ mbMaster.writeMultipleCoils(2, mb2_write_coil_startAddress, coilBool_Write[1].coil, mb2_write_coil_length); mb2_write_coil = false; }
    xSemaphoreGive(xSemaphore);
    vTaskDelay(pdMS_TO_TICKS(1));
  }
}
void TaskslaveId1_write_rhd(void *pvParameters) {
  (void)pvParameters; while (1) {
    xSemaphoreTake(xSemaphore, portMAX_DELAY);
      if(mb1_write_rhd){ mbMaster.writeMultipleHoldingRegisters(1, mb1_write_rhd_startAddress, mbFloat_Write[0].words, mb1_write_rhd_length); mb1_write_rhd = false; }
    xSemaphoreGive(xSemaphore);
    vTaskDelay(pdMS_TO_TICKS(1));
  }
}
void TaskslaveId2_write_rhd(void *pvParameters){
  (void)pvParameters; while (1) {
    xSemaphoreTake(xSemaphore, portMAX_DELAY);
      if(mb2_write_rhd){ mbMaster.writeMultipleHoldingRegisters(2, mb2_write_rhd_startAddress, mbFloat_Write[1].words, mb2_write_rhd_length); mb2_write_rhd = false; }
    xSemaphoreGive(xSemaphore);
    vTaskDelay(pdMS_TO_TICKS(1));
  }
}
void read_slave(uint8_t id){
  if(countStepMbmaster[id-1] ++ > 3) countStepMbmaster[id-1] = 0;
  switch (countStepMbmaster[id-1]){
  case 1: mbMaster.readHoldingRegisters(id, 0, mbFloat[id-1].words, 25); break;
  case 2: mbMaster.readCoils(id, 0, coilBool[id-1].coil, 10); break;
  case 3: mbMaster.readDiscreteInputs(id, 0, coilBool[id-1].inputDiscrete, 2); break; }
  digitalWrite(pin_led, stateLed = !stateLed);
}
uint8_t mb1_ReadDiscreteInput(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb1_sizeDiscreateInput || (address + length) > mb1_sizeDiscreateInput) return STATUS_ILLEGAL_DATA_ADDRESS;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(index < 2) mbs.writeDiscreteInputToBuffer(i, coilBool[0].inputDiscrete[index]);
    else if(index >= 2 && index < 8) mbs.writeDiscreteInputToBuffer(i, !digitalRead(X[index - 2]));
    else if(index == 8) mbs.writeDiscreteInputToBuffer(i, IsConnectTCP);
    else mbs.writeDiscreteInputToBuffer(i, stateLed);
  }
  return STATUS_OK;
}
uint8_t mb1_Coil(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb1_sizeCoil || (address + length) > mb1_sizeCoil) return STATUS_ILLEGAL_DATA_ADDRESS;
  bool IsWrite = false;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(fc == FC_READ_COILS){
      if(index < 10) mbs.writeCoilToBuffer(i, coilBool[0].coil[index]);
      else if(index < 16 && index >= 10) mbs.writeCoilToBuffer(i, digitalRead(Y[index - 10]));
      else mbs.writeCoilToBuffer(i, mb_flagCoil[index - 16]);
    }else{
      if(index < 10){ coilBool_Write[0].coil[i] = mbs.readCoilFromBuffer(i); IsWrite = true; }
      else if(index < 16 && index >= 10) digitalWrite(Y[index - 10], mbs.readCoilFromBuffer(i));
      else mb_flagCoil[index - 16] = mbs.readCoilFromBuffer(i);
    }
  }
  if(!IsWrite) return STATUS_OK;
  mb1_write_coil_length = ((address + length) >= 10) ? (length - ((address + length) - 10)) : length;
  mb1_write_coil_startAddress = address;
  mb1_write_coil = true;
  return STATUS_OK;
}
uint8_t mb1_HoldRegister(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb1_sizeHoldingRegister || (address + length) > mb1_sizeHoldingRegister) return STATUS_ILLEGAL_DATA_ADDRESS;
  bool IsWrite = false;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(fc == FC_READ_HOLDING_REGISTERS){
      if(index < 25) mbs.writeRegisterToBuffer(i, mbFloat[0].words[index]);
      else mbs.writeRegisterToBuffer(i, mb_holdRegister[index - 25]);
    }else{
      if(index < 25){ mbFloat_Write[0].words[i] = mbs.readRegisterFromBuffer(i); IsWrite = true; }
      else mb_holdRegister[index - 25] = mbs.readRegisterFromBuffer(i);
    }
  }
  if(!IsWrite) return STATUS_OK;
  mb1_write_rhd_length = ((address + length) >= 25) ? (length - ((address + length) - 25)) : length;
  mb1_write_rhd_startAddress = address;
  mb1_write_rhd = true;
  return STATUS_OK;
}
uint8_t mb2_ReadDiscreteInput(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb2_sizeDiscreateInput || (address + length) > mb2_sizeDiscreateInput) return STATUS_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < length; i++){
      uint8_t index = address + i;
      if(index < 2) mbs.writeDiscreteInputToBuffer(i, coilBool[1].inputDiscrete[index]);
      else if(index >= 2 && index < 8) mbs.writeDiscreteInputToBuffer(i, !digitalRead(X[index - 2]));
      else if(index == 8) mbs.writeDiscreteInputToBuffer(i, IsConnectTCP);
      else mbs.writeDiscreteInputToBuffer(i, stateLed);
  }
  return STATUS_OK;
}
uint8_t mb2_Coil(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb2_sizeCoil || (address + length) > mb2_sizeCoil) return STATUS_ILLEGAL_DATA_ADDRESS;
  bool IsWrite = false;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(fc == FC_READ_COILS){
      if(index < 10) mbs.writeCoilToBuffer(i, coilBool[1].coil[index]);
      else if(index < 16 && index >= 10) mbs.writeCoilToBuffer(i, digitalRead(Y[index - 10]));
      else mbs.writeCoilToBuffer(i, mb_flagCoil[index - 16]);
    }else{
      if(index < 10){ coilBool_Write[1].coil[i] = mbs.readCoilFromBuffer(i); IsWrite = true; }
      else if(index < 16 && index >= 10) digitalWrite(Y[index - 10], mbs.readCoilFromBuffer(i));
      else mb_flagCoil[index - 16] = mbs.readCoilFromBuffer(i);
    }
  }
  if(!IsWrite) return STATUS_OK;
  mb2_write_coil_length = ((address + length) >= 10) ? (length - ((address + length) - 10)) : length;
  mb2_write_coil_startAddress = address;
  mb2_write_coil = true;
  return STATUS_OK;
}
uint8_t mb2_HoldRegister(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mb2_sizeHoldingRegister || (address + length) > mb2_sizeHoldingRegister) return STATUS_ILLEGAL_DATA_ADDRESS;
  bool IsWrite = false;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(fc == FC_READ_HOLDING_REGISTERS){
      if(index < 25) mbs.writeRegisterToBuffer(i, mbFloat[1].words[index]);
      else mbs.writeRegisterToBuffer(i, mb_holdRegister[index - 25]);
    }else{
      if(index < 25){ mbFloat_Write[1].words[i] = mbs.readRegisterFromBuffer(i); IsWrite = true; }
      else mb_holdRegister[index - 25] = mbs.readRegisterFromBuffer(i);
    }
  }
  if(!IsWrite) return STATUS_OK;
  mb2_write_rhd_length = ((address + length) >= 25) ? (length - ((address + length) - 25)) : length;
  mb2_write_rhd_startAddress = address;
  mb2_write_rhd = true;
  return STATUS_OK;
}
bool itemDisCreated(const char *itemID, const opcOperation opcOP, const bool value){
  Serial.println(itemID);
  timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
  if(opcOP != (enum opcOperation)opc_read) return false;
  for (uint8_t i = 0; i < size_item_D; i++){
    if(i < 2 && !strcmp(itemID, ItemD[i])) return coilBool[0].inputDiscrete[i];
    else if(i >= 2 && i < 4 && !strcmp(itemID, ItemD[i])) return coilBool[1].inputDiscrete[i-2];
    else if(i >= 4 && i < 10 && !strcmp(itemID, ItemD[i])) return !digitalRead(X[i - 4]);
    else if(i == 10 && !strcmp(itemID, ItemD[i])) return IsConnectTCP;
    else if(i == 11 && !strcmp(itemID, ItemD[i])) return stateLed;
  } return false;
}
bool itemDisCoil(const char *itemID, const opcOperation opcOP, const bool value){
  timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
  if(opcOP == (enum opcOperation)opc_write){
    for (uint8_t i = 0; i < size_item_C; i++){
      if(i < 10 && !strcmp(itemID, ItemC[i])){ coilBool_Write[0].coil[i] = value; mb1_write_coil_length = 1; mb1_write_coil_startAddress = i; mb1_write_coil = true; return coilBool_Write[0].coil[i]; }
      if(i >= 10 && i < 20 && !strcmp(itemID, ItemC[i])){ coilBool_Write[1].coil[i - 10] = value; mb2_write_coil_length = 1; mb2_write_coil_startAddress = i - 10; mb2_write_coil = true; return coilBool_Write[1].coil[i - 10]; }
      else if(i >= 20 && i < 26 && !strcmp(itemID, ItemC[i])) { digitalWrite(Y[i - 20], value); return digitalRead(Y[i - 20]); }
      else if(i >= 26 && i < 40 && !strcmp(itemID, ItemC[i])) { mb_flagCoil[i-26] = value; return mb_flagCoil[i-26]; }
    }
  }else{
    Serial.println(itemID);
    for (uint8_t i = 0; i < size_item_C; i++){
      if(i < 10 && !strcmp(itemID, ItemC[i])) return coilBool[0].coil[i];
      if(i >= 10 && i < 20 && !strcmp(itemID, ItemC[i])) return coilBool[1].coil[i-10];
      else if(i >= 20 && i < 26 && !strcmp(itemID, ItemC[i])) return digitalRead(Y[i - 20]);
      else if(i >= 26 && i < 40 && !strcmp(itemID, ItemC[i])) return mb_flagCoil[i-26];
    }
  } return false;
}
int itemInt(const char *itemID, const opcOperation opcOP, const int value){
  timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
  if(opcOP == (enum opcOperation)opc_write){
    for (uint8_t i = 0; i < size_item_I; i++){
      if(i < 5 && !strcmp(itemID, ItemI[i])){ mbFloat_Write[0].words[i+20] = value; mb1_write_rhd_length = 1; mb1_write_rhd_startAddress = i+20; mb1_write_rhd = true; return mbFloat_Write[0].words[i+20]; }
      else if(i >= 5 && i < 10 && !strcmp(itemID, ItemI[i])){ mbFloat_Write[1].words[i - 5 + 20] = value; mb2_write_rhd_length = 1; mb2_write_rhd_startAddress = i - 5 + 20; mb2_write_rhd = true; return mbFloat_Write[1].words[i+20]; }
      else if(i >= 10 && i < 15 && !strcmp(itemID, ItemI[i])) mb_holdRegister[i-10] = value; return mb_holdRegister[i-10];
    }
  }else{
    Serial.println(itemID);
    for (uint8_t i = 0; i < size_item_I; i++){
      if(i < 5 && !strcmp(itemID, ItemI[i])) return mbFloat[0].words[i + 20];
      else if(i >= 5 && i < 10 && !strcmp(itemID, ItemI[i])) return mbFloat[1].words[i - 5 + 20];
      else if(i >= 10 && i < 15 && !strcmp(itemID, ItemI[i])) return mb_holdRegister[i-10];
    }
  }
  return 0;
}
float itemFloat(const char *itemID, const opcOperation opcOP, const float value){
  timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
  if(opcOP == (enum opcOperation)opc_write){
    for (uint8_t i = 0; i < size_item_F; i++){
      if(i < 10 && !strcmp(itemID, ItemF[i])){
        switch (i) {
        case 0: mbFloat_Write[0].values.kFact = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.kFact;  break;
        case 1: mbFloat_Write[0].values.capacity = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.capacity; break;
        case 2: mbFloat_Write[0].values.setPoint = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.setPoint; break;
        case 3: mbFloat_Write[0].values.factorKurang = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.factorKurang; break;
        case 4: mbFloat_Write[0].values.liter = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.liter; break;
        case 5: mbFloat_Write[0].values.sec = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.sec; break;
        case 6: mbFloat_Write[0].values.freq = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.freq; break;
        case 7: mbFloat_Write[0].values.Correct = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.Correct; break;
        case 8: mbFloat_Write[0].values.Flowrate = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.Flowrate; break;
        case 9: mbFloat_Write[0].values.Volume = value; mb1_write_rhd_length = 2; mb1_write_rhd_startAddress = i * 2; mb1_write_rhd = true; return mbFloat_Write[0].values.Volume; break;
        }
      } else if(i >= 10 && i < 20 && !strcmp(itemID, ItemF[i])){
        switch (i - 10) {
        case 0: mbFloat_Write[1].values.kFact = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.kFact;  break;
        case 1: mbFloat_Write[1].values.capacity = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.capacity; break;
        case 2: mbFloat_Write[1].values.setPoint = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.setPoint; break;
        case 3: mbFloat_Write[1].values.factorKurang = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.factorKurang; break;
        case 4: mbFloat_Write[1].values.liter = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.liter; break;
        case 5: mbFloat_Write[1].values.sec = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.sec; break;
        case 6: mbFloat_Write[1].values.freq = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.freq; break;
        case 7: mbFloat_Write[1].values.Correct = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.Correct; break;
        case 8: mbFloat_Write[1].values.Flowrate = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.Flowrate; break;
        case 9: mbFloat_Write[1].values.Volume = value; mb2_write_rhd_length = 2; mb2_write_rhd_startAddress = (i - 10) * 2; mb2_write_rhd = true; return mbFloat_Write[1].values.Volume; break;
        }
      }
    }
  }else{
    Serial.println(itemID);
    for (uint8_t i = 0; i < size_item_F; i++){
      if(i < 10 && !strcmp(itemID, ItemF[i])){
        switch (i) {
        case 0: return mbFloat[0].values.kFact;  break;
        case 1: return mbFloat[0].values.capacity; break;
        case 2: return mbFloat[0].values.setPoint; break;
        case 3: return mbFloat[0].values.factorKurang; break;
        case 4: return mbFloat[0].values.liter; break;
        case 5: return mbFloat[0].values.sec; break;
        case 6: return mbFloat[0].values.freq; break;
        case 7: return mbFloat[0].values.Correct; break;
        case 8: return mbFloat[0].values.Flowrate; break;
        case 9: return mbFloat[0].values.Volume; break;
        }
      } else if(i >= 10 && i < 20 && !strcmp(itemID, ItemF[i])){
        switch (i - 10) {
        case 0: return mbFloat[1].values.kFact;  break;
        case 1: return mbFloat[1].values.capacity; break;
        case 2: return mbFloat[1].values.setPoint; break;
        case 3: return mbFloat[1].values.factorKurang; break;
        case 4: return mbFloat[1].values.liter; break;
        case 5: return mbFloat[1].values.sec; break;
        case 6: return mbFloat[1].values.freq; break;
        case 7: return mbFloat[1].values.Correct; break;
        case 8: return mbFloat[1].values.Flowrate; break;
        case 9: return mbFloat[1].values.Volume; break;
        }
      }
    }
  }
  return 0.0f;
}