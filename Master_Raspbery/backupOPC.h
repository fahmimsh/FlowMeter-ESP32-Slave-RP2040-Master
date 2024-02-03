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


uint8_t idFlow = 1, idOPC_RP2040 = 0; IPAddress ip(192, 168, 1, 123);
//uint8_t idFlow = 2, idOPC_RP2040 = 1; IPAddress ip(192, 168, 1, 124);
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
mbFloatInt mbFloat[MaxId], mbFloat_Write_OPC[MaxId];
struct CoilBool { bool coil[5]; bool inputDiscrete[2]; };
CoilBool coilBool[MaxId], coilBool_Write_OPC[MaxId];
uint8_t countStepMbmaster[MaxId], flagWriteMbMasterRH[MaxId], flagWriteMbMasterCoil[MaxId];
uint8_t startAddresWriteCoil[MaxId], startAddresWriteRH[MaxId], startAddresWriteRH_length[MaxId];

unsigned long timePrevIsConnectTCP;
bool stateLed, IsConnectTCP, mb_flagCoil[12];
uint8_t mb_sizeDiscreateInput = ((sizeof(coilBool[0].inputDiscrete) / sizeof(coilBool[0].inputDiscrete[0])) * MaxId) + (sizeof(X) / sizeof(X[0]) + 1);
uint8_t mb_sizeCoil = ((sizeof(coilBool[0].coil) / sizeof(coilBool[0].coil[0])) * MaxId) + (sizeof(Y) / sizeof(Y[0])) + (sizeof(mb_flagCoil) / sizeof(mb_flagCoil[0]));
uint8_t mb_sizeHoldingRegister = ((sizeof(mbFloat[0].words) / sizeof(mbFloat[0].words[0])) * MaxId);

OPCEthernet opc;
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
bool itemDisCreated(const char *itemID, const opcOperation opcOP, const bool value);
bool itemDisCoil(const char *itemID, const opcOperation opcOP, const bool value);
int itemInt(const char *itemID, const opcOperation opcOP, const int value);
float itemFloat(const char *itemID, const opcOperation opcOP, const float value);
void setup() {
  Serial.begin(115200);
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
  for (uint8_t i = 0; i < 6; i++){ btn[i].attach(X[i], INPUT); btn[i].interval(50); btn[i].setPressedState(LOW); pinMode(Y[i], OUTPUT); } pinMode(pin_led, OUTPUT);
  opc.setup(80, idOPC_RP2040, ip, dns_server, gateway, subnet);
  for (uint8_t i = 0; i < size_item_D; i++) opc.addItem(ItemD[i], opc_read, opc_bool, itemDisCreated);
  for (uint8_t i = 0; i < size_item_C; i++) opc.addItem(ItemC[i], opc_readwrite, opc_bool, itemDisCoil);
  for (uint8_t i = 0; i < size_item_I; i++) opc.addItem(ItemI[i], opc_readwrite, opc_int, itemInt);
  for (uint8_t i = 0; i < size_item_F; i++) opc.addItem(ItemF[i], opc_readwrite, opc_float, itemFloat);
  xSemaphore = xSemaphoreCreateMutexStatic(&xMutexBuffer);
  xTaskCreateStatic(TaskslaveId1, "TaskslaveId1", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveid1, &xTaskBuffer_slaveid1);
  xTaskCreateStatic(TaskslaveId2, "TaskslaveId2", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveid2, &xTaskBuffer_slaveid2);
  xTaskCreateStatic(TaskslaveIdWrite, "TaskslaveIdWrite", 400, NULL, configMAX_PRIORITIES - 1, xStack_slaveidWrite, &xTaskBuffer_slaveidWrite);
}
void loop() {
  if(IsConnectTCP && millis() - timePrevIsConnectTCP >= 5000) IsConnectTCP = false;
  opc.processOPCCommands();
  mb->poll();
  vTaskDelay(pdMS_TO_TICKS(1)); // Ganti delay dengan vTaskDelay
}
void TaskslaveId1(void *pvParameters) { (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); read_slave(1); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(100)); } }
void TaskslaveId2(void *pvParameters) { (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); read_slave(2); xSemaphoreGive(xSemaphore); vTaskDelay(pdMS_TO_TICKS(100)); } }
void TaskslaveIdWrite(void *pvParameters) { 
  (void)pvParameters; while (1) { xSemaphoreTake(xSemaphore, portMAX_DELAY); 
  if(flagWriteMbMasterCoil[0]){ mbMaster.writeSingleCoil(1, startAddresWriteCoil[0], coilBool_Write_OPC[0].coil[startAddresWriteCoil[0]]); flagWriteMbMasterCoil[0] = false; }
  if(flagWriteMbMasterCoil[1]){ mbMaster.writeSingleCoil(2, startAddresWriteCoil[1], coilBool_Write_OPC[1].coil[startAddresWriteCoil[1]]); flagWriteMbMasterCoil[1] = false; }
  if(flagWriteMbMasterRH[0]){ mbMaster.writeMultipleHoldingRegisters(1, startAddresWriteRH[0], mbFloat_Write_OPC[0].words, startAddresWriteRH_length[0]); flagWriteMbMasterRH[0] = false; }
  if(flagWriteMbMasterRH[1]){ mbMaster.writeMultipleHoldingRegisters(2, startAddresWriteRH[1], mbFloat_Write_OPC[1].words, startAddresWriteRH_length[1]); flagWriteMbMasterRH[1] = false; }
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
        coilBool_Write_OPC[0].coil[index] = mb->readCoilFromBuffer(i); startAddresWriteCoil[0] = index;
        flagWrite1 = true; 
      }
      else if(index >= 5 && index < 10) { 
        coilBool_Write_OPC[1].coil[index - 5] = mb->readCoilFromBuffer(i); startAddresWriteCoil[1] = index - 5;
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
        mbFloat_Write_OPC[0].words[i] = mb->readRegisterFromBuffer(i); startAddresWriteRH[0] = index >= 12 ? index : index - 1; startAddresWriteRH_length[0] = length;
        flagWrite1 = true; 
      }
      else if(index >= 15 && index < 30){ 
        mbFloat_Write_OPC[1].words[i] = mb->readRegisterFromBuffer(i); startAddresWriteRH[1] = (index - 15) >= 12 ? (index - 15) : (index - 16); startAddresWriteRH_length[1] = length;
        flagWrite2 = true; 
      }
    }
  }
  if(flagWrite1) {flagWriteMbMasterRH[0] = true;}
  if(flagWrite2) {flagWriteMbMasterRH[1] = true;}
  return STATUS_OK;
}
bool itemDisCreated(const char *itemID, const opcOperation opcOP, const bool value){
  timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
  if(opcOP) return false;
  for (uint8_t i = 0; i < size_item_D; i++){
    if(i < 2 && !strcmp(itemID, ItemD[i])) return coilBool[0].inputDiscrete[i];
    else if(i >= 2 && i < 4 && !strcmp(itemID, ItemD[i])) return coilBool[1].inputDiscrete[i-2];
    else if(i >= 4 && i < 10 && !strcmp(itemID, ItemD[i])) return !digitalRead(X[i - 4]);
  } return false;
}
bool itemDisCoil(const char *itemID, const opcOperation opcOP, const bool value){
  timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
  if(opcOP){
    Serial.print(value); Serial.println(itemID);
    for (uint8_t i = 0; i < size_item_C; i++){
      if(i < 5 && !strcmp(itemID, ItemC[i])) { coilBool_Write_OPC[0].coil[i] = value; startAddresWriteCoil[0] = i; flagWriteMbMasterCoil[0] = true;  return value; }
      if(i >= 5 && i < 10 && !strcmp(itemID, ItemC[i])) { coilBool_Write_OPC[1].coil[i - 5] = value; startAddresWriteCoil[1] = i - 5; flagWriteMbMasterCoil[1] = true; return value; }
      else if(i >= 10 && i < 16 && !strcmp(itemID, ItemC[i])) { digitalWrite(Y[i - 10], value); return digitalRead(Y[i - 10]); }
      else if(i >= 16 && i < 28 && !strcmp(itemID, ItemC[i])) { mb_flagCoil[i-16] = value; return mb_flagCoil[i-16]; }
    }
  }else{
    for (uint8_t i = 0; i < size_item_C; i++){
      if(i < 5 && !strcmp(itemID, ItemC[i])) return coilBool[0].coil[i];
      if(i >= 5 && i < 10 && !strcmp(itemID, ItemC[i])) return coilBool[1].coil[i-5];
      else if(i >= 10 && i < 16 && !strcmp(itemID, ItemC[i])) return digitalRead(Y[i - 10]);
      else if(i >= 16 && i < 28 && !strcmp(itemID, ItemC[i])) return mb_flagCoil[i-16];
    }
  } return false;
}
int itemInt(const char *itemID, const opcOperation opcOP, const int value){
  timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
  if(opcOP){
    Serial.println(itemID);
    for (uint8_t i = 0; i < size_item_I; i++){
      if(i < 3 && !strcmp(itemID, ItemI[i])) { 
        mbFloat_Write_OPC[0].words[0] = value; 
        startAddresWriteRH[0] = i+12;
        startAddresWriteRH_length[0] = 1;
        flagWriteMbMasterRH[0] = true; 
        return value; 
      }
      else if(i >= 3 && i < 6 && !strcmp(itemID, ItemI[i])) { 
        mbFloat_Write_OPC[1].words[i] = value; 
        startAddresWriteRH[1] = i + 9;
        startAddresWriteRH_length[1] = 1;
        flagWriteMbMasterRH[1] = true; 
        return value; 
      }
    }
  }else{
    for (uint8_t i = 0; i < size_item_I; i++){
      if(i < 3 && !strcmp(itemID, ItemI[i])) return mbFloat[0].words[i + 13];
      else if(i >= 3 && i < 6 && !strcmp(itemID, ItemI[i])) return mbFloat[1].words[i - 3 + 13];
    }
  }
  return value;
}
float itemFloat(const char *itemID, const opcOperation opcOP, const float value){
  timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
  if(opcOP){
    Serial.println(itemID); memcpy(mbFloat_Write_OPC[0].words, mbFloat[0].words, sizeof(mbFloat[0].words)); memcpy(mbFloat_Write_OPC[1].words, mbFloat[1].words, sizeof(mbFloat[1].words));
    for (uint8_t i = 0; i < size_item_F; i++){
      if(!strcmp(itemID, ItemF[i])){
        if(i < 6){
          mbFloat_Write_OPC[0].values.kFact = value; startAddresWriteRH[0] = i * 2; startAddresWriteRH_length[0] = 2; flagWriteMbMasterRH[0] = true;
        }else{
          mbFloat_Write_OPC[1].values.kFact = value; startAddresWriteRH[1] = (i - 6) * 2; startAddresWriteRH_length[1] = 2; flagWriteMbMasterRH[1] = true; 
        }
        return value;
      }
    }
  }else{
    for (uint8_t i = 0; i < size_item_F; i++){
      if(!strcmp(itemID, ItemF[i])){
        switch (i) {
        case 0: return mbFloat[0].values.kFact;  break;
        case 1: return mbFloat[0].values.capacity; break;
        case 2: return mbFloat[0].values.setPoint; break;
        case 3: return mbFloat[0].values.factorKurang; break;
        case 4: return mbFloat[0].values.liter; break;
        case 5: return mbFloat[0].values.Flowrate; break;
        case 6: return mbFloat[1].values.kFact;  break;
        case 7: return mbFloat[1].values.capacity; break;
        case 8: return mbFloat[1].values.setPoint; break;
        case 9: return mbFloat[1].values.factorKurang; break;
        case 10: return mbFloat[1].values.liter; break;
        case 11: return mbFloat[1].values.Flowrate; break;
        }
      }
    }
  }
  return value;
}