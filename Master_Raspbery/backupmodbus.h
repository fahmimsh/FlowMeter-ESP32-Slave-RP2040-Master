; PlatformIO Project Configuration File
;
;   Build options: build flags, source filter
;   Upload options: custom upload port, speed and extra flags
;   Library options: dependencies, extra library storages
;   Advanced options: extra scripting
;
; Please visit documentation for the other options and examples
; https://docs.platformio.org/page/projectconf.html

[env:pico]
platform = https://github.com/maxgerhardt/platform-raspberrypi.git
board = pico
framework = arduino
board_build.core = earlephilhower
board_build.f_cpu = 133000000L
monitor_speed = 115200
check_tool = cppcheck
lib_deps =
    cmb27/ModbusRTUMaster@^1.0.5
	thomasfredericks/Bounce2@^2.72
    yaacov/ModbusSlave@^2.1.1

#include <Arduino.h>
#include <FreeRTOS.h>
#include <task.h>
#include <semphr.h>
#include <ModbusSlave.h>
#include <ModbusRTUMaster.h>
#include <Bounce2.h>

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

Bounce2::Button btn[6];
ModbusRTUMaster mbMaster(Serial1);
ModbusSlave mb[2] = {ModbusSlave(1), ModbusSlave(2)};
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
  xSemaphore = xSemaphoreCreateMutexStatic(&xMutexBuffer);
  xTaskCreateStatic(TaskslaveId1, "TaskslaveId1", 400, NULL, configMAX_PRIORITIES - 2, xStack_slaveid1, &xTaskBuffer_slaveid1);
  xTaskCreateStatic(TaskslaveId2, "TaskslaveId2", 400, NULL, configMAX_PRIORITIES - 2, xStack_slaveid2, &xTaskBuffer_slaveid2);
  xTaskCreateStatic(TaskslaveId1_write_coil, "TaskslaveId1_write_coil", 200, NULL, configMAX_PRIORITIES - 1, xStack_slaveid1_write_coil, &xTaskBuffer_slaveid1_write_coil);
  xTaskCreateStatic(TaskslaveId2_write_coil, "TaskslaveId2_write_coil", 200, NULL, configMAX_PRIORITIES - 1, xStack_slaveid2_write_coil, &xTaskBuffer_slaveid2_write_coil);
  xTaskCreateStatic(TaskslaveId1_write_rhd, "TaskslaveId1_write_rhd", 200, NULL, configMAX_PRIORITIES - 1, xStack_slaveid1_write_rhd, &xTaskBuffer_slaveid1_write_rhd);
  xTaskCreateStatic(TaskslaveId2_write_rhd, "TaskslaveId2_write_rhd", 200, NULL, configMAX_PRIORITIES - 1, xStack_slaveid2_write_rhd, &xTaskBuffer_slaveid2_write_rhd);
}
void loop() {
  mbs.poll();
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
    else if(index >= 2 && index < 8) mbs.writeDiscreteInputToBuffer(i, digitalRead(X[index - 2]));
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
      else if(index >= 2 && index < 8) mbs.writeDiscreteInputToBuffer(i, digitalRead(X[index - 2]));
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