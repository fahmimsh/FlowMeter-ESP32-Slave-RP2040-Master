#include <Arduino.h>
#include <math.h>
#include <ModbusSlave.h>
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <esp_task_wdt.h>
#include <HardwareSerial.h>
#include <Preferences.h>
#include <nvs_flash.h>
#include <Bounce2.h>

const int X[4] = {2, 4, 21, 22};
const int Y[3] = {23, 25, 26};
volatile int count = 0, pulse = 0;
volatile bool flagTime = false;
unsigned long prevMillis;
union mbFloatInt {
  struct {
    float kFact; //0, 1
    float capacity; //2, 3
    float setPoint; //4, 5
    float factorKurang; // 6, 7
    float liter; //8, 9
    float sec; //10, 11
    float freq; // 12, 13
    float Correct; //14, 15
    float Flowrate; //16, 17
    float Volume; //18, 19
    uint16_t slave_id; //20
    uint16_t baudrate; //21
    uint16_t timeInterval_OnValve; //22
    uint16_t timeInterval_OffValve; //23
    uint16_t over_fl_err; //24
  } values;
  uint16_t words[25];
};
mbFloatInt mbFloat;
/* OnOffFlow, FlagLog, flagOn, flagOff, flagEventOnOff, flagOffPrev, flagPrintserial */
bool flagCoil[7] = {false, false, false, false, false, false, false};
uint8_t mbFloatIntSize = sizeof(mbFloat.words) / sizeof(mbFloat.words[0]), flagCoilSize = (sizeof(flagCoil) / sizeof(flagCoil[0])) + (sizeof(Y) / sizeof(Y[0]));
unsigned long timePrevOn, timePrevOff;

Bounce2::Button btn;
Preferences prefs;
Modbus *mb;
hw_timer_t *timer = NULL;
hw_timer_t *timerInterval = NULL;
portMUX_TYPE synch = portMUX_INITIALIZER_UNLOCKED;
portMUX_TYPE timerMux = portMUX_INITIALIZER_UNLOCKED;
void IRAM_ATTR plsRising() { portENTER_CRITICAL(&synch); count++; portEXIT_CRITICAL(&synch); }
void IRAM_ATTR plsFalling() { portENTER_CRITICAL(&synch); count++; portEXIT_CRITICAL(&synch); }
void IRAM_ATTR onTimer() { portENTER_CRITICAL_ISR(&timerMux); flagTime = true; portEXIT_CRITICAL_ISR(&timerMux); }
float formulaLiter(double &volume){
  float setPoint_fl = mbFloat.values.setPoint - mbFloat.values.factorKurang;
  if(digitalRead(Y[0]) && mbFloat.values.liter >= setPoint_fl && digitalRead(X[3])){
    static float target_err_over_fl = 0.0f;
    if(flagCoil[0]){
      flagCoil[0] = false;
      flagCoil[4] = true;
      mbFloat.values.liter += volume;
      target_err_over_fl = mbFloat.values.liter - mbFloat.values.setPoint;
    }
    return mbFloat.values.liter + abs(volume/(static_cast<float>(mbFloat.values.over_fl_err)/10.0f));
  } else return mbFloat.values.liter + volume;
}
void calculate(void *pvParameters) {
  (void)pvParameters;
  for (;;) {
    if(flagTime){
      portENTER_CRITICAL_ISR(&timerMux); flagTime = false; portEXIT_CRITICAL_ISR(&timerMux);
      portENTER_CRITICAL(&synch); pulse = count; count = 0; portEXIT_CRITICAL(&synch);
      unsigned long mils = millis(); 
      double _sec = (mils  - prevMillis) / 1000.0f;
      double _freq = pulse / _sec;
      unsigned int _decile = floor(10.0f * _freq / (mbFloat.values.capacity * mbFloat.values.kFact));
      prevMillis = mils;
      double mFactor[10] = {1,1,1,1,1,1,1,1,1,1};
      unsigned int ceiling =  9; 
      double _Correct = mbFloat.values.kFact / mFactor[min(_decile, ceiling)];
      double _Flowrate = _freq / _Correct;
      double _Volume = _Flowrate / (60.0f/_sec);
      mbFloat.values.sec = _sec;
      mbFloat.values.freq = _freq;
      mbFloat.values.Correct = _Correct;
      mbFloat.values.Flowrate = _Flowrate;
      mbFloat.values.Volume = _Volume;
      mbFloat.values.liter = formulaLiter(_Volume);
    }
    vTaskDelay(1/portTICK_PERIOD_MS); // Delay 1ms, adjust as needed
  }
}
void InterruptPinChangeMode(bool FlagInit){    
  if(FlagInit){
      attachInterrupt(digitalPinToInterrupt(X[0]), plsRising, FALLING);
      attachInterrupt(digitalPinToInterrupt(X[1]), plsFalling, RISING);
    return;
  }
  detachInterrupt(digitalPinToInterrupt(X[0])); detachInterrupt(digitalPinToInterrupt(X[1]));
}
void prefereces_partition(bool ReadOrWrite){
  prefs.begin("file-app", false);
  if(ReadOrWrite){
    mbFloat.values.kFact = prefs.getFloat("kFact", 500.0f);
    mbFloat.values.capacity = prefs.getFloat("capacity", 500.0f);
    mbFloat.values.setPoint = prefs.getFloat("setPoint", 100.0f);
    mbFloat.values.factorKurang = prefs.getFloat("factorKurang", 0.8f);
    mbFloat.values.slave_id = prefs.getUInt("slave_id", 1);
    mbFloat.values.baudrate = prefs.getUInt("baudrate", 9600);
    mbFloat.values.timeInterval_OnValve = prefs.getUInt("time-OnValve", 200);
    mbFloat.values.timeInterval_OffValve = prefs.getUInt("time-OffValve", 200);
    mbFloat.values.over_fl_err = prefs.getUInt("over_fl_err", 10);
  }else{
    prefs.putFloat("kFact", mbFloat.values.kFact);
    prefs.putFloat("capacity", mbFloat.values.capacity);
    prefs.putFloat("setPoint", mbFloat.values.setPoint);
    prefs.putFloat("factorKurang", mbFloat.values.factorKurang);
    prefs.putUInt("slave_id", mbFloat.values.slave_id);
    prefs.putUInt("baudrate", mbFloat.values.baudrate);
    prefs.putUInt("time-OnValve", mbFloat.values.timeInterval_OnValve);
    prefs.putUInt("time-OffValve", mbFloat.values.timeInterval_OffValve);
    prefs.putUInt("over_fl_err", mbFloat.values.over_fl_err);
  }
  prefs.end();
}
void printSerialMbResponse(bool &IsOn, uint8_t &fc, uint16_t &address, uint16_t &length){
  if(!IsOn) return;
  Serial.print(fc); Serial.print(" "); Serial.print(address); Serial.print(" "); Serial.print(length); Serial.println(" OK");
}
uint8_t mb_ReadDiscreteInput(uint8_t fc, uint16_t address, uint16_t length){
  if (address > 2 || (address + length) > 2) return STATUS_ILLEGAL_DATA_ADDRESS;
  for (int i = 0; i < length; i++){
    mb->writeDiscreteInputToBuffer(i, digitalRead(X[address + i + 2]));
  }
  printSerialMbResponse(flagCoil[6], fc, address, length);
  return STATUS_OK;
}
uint8_t mb_Coil(uint8_t fc, uint16_t address, uint16_t length){
  if (address > flagCoilSize || (address + length) > flagCoilSize) return STATUS_ILLEGAL_DATA_ADDRESS;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(fc == FC_READ_COILS){
      if(index < 3) mb->writeCoilToBuffer(i, digitalRead(Y[index]));
      else mb->writeCoilToBuffer(i, flagCoil[index - 3]);
    }else{
      if(index < 3) digitalWrite(Y[index], mb->readCoilFromBuffer(i));
      else{
        flagCoil[index - 3] = mb->readCoilFromBuffer(i);
        if(index == 3){
          if(digitalRead(X[3]) && digitalRead(Y[0])){
            if(!flagCoil[0]) flagCoil[0] = !flagCoil[0];
          }else{
            flagCoil[4] = true;
          }
        }
      }
    }
  }
  printSerialMbResponse(flagCoil[6], fc, address, length);
  return STATUS_OK;
}
uint8_t mb_HoldRegister(uint8_t fc, uint16_t address, uint16_t length){
  if (address > mbFloatIntSize || (address + length) > mbFloatIntSize) return STATUS_ILLEGAL_DATA_ADDRESS;
  for (int i = 0; i < length; i++){
    uint8_t index = address + i;
    if(fc == FC_READ_HOLDING_REGISTERS){
      mb->writeRegisterToBuffer(i, mbFloat.words[index]);
    }else{
      mbFloat.words[index] = mb->readRegisterFromBuffer(i);
    }
  }
  if((address >= 0 && address < 8 && fc != FC_READ_HOLDING_REGISTERS) || (address >= 20 && address < 25 && fc != FC_READ_HOLDING_REGISTERS)) prefereces_partition(false);
  printSerialMbResponse(flagCoil[6], fc, address, length);
  return STATUS_OK;
}
void setup() {
  //nvs_flash_erase(); nvs_flash_init(); while(true); //erase the NVS partition
  //mbFloat.values.kFact = 500.0f; mbFloat.values.capacity = 500.0f; mbFloat.values.setPoint = 100.0f; mbFloat.values.slave_id = 1; mbFloat.values.baudrate = 9600; mbFloat.values.timeInterval_OnValve = 200; mbFloat.values.timeInterval_OffValve = 200;
  Serial.begin(9600);
  prefereces_partition(true);
  for (int i = 0; i < 4; i++){ if(i < 3) pinMode(Y[i], OUTPUT); if(i > 1) pinMode(X[i], INPUT);}
  esp_task_wdt_init(75, true); esp_task_wdt_add(NULL);
  mb = new Modbus(Serial2, mbFloat.values.slave_id, 13);
  mb->cbVector[CB_READ_DISCRETE_INPUTS] = mb_ReadDiscreteInput;
  mb->cbVector[CB_READ_COILS] = mb_Coil;
  mb->cbVector[CB_WRITE_COILS] = mb_Coil;
  mb->cbVector[CB_READ_HOLDING_REGISTERS] = mb_HoldRegister;
  mb->cbVector[CB_WRITE_HOLDING_REGISTERS] = mb_HoldRegister;
  Serial2.begin(mbFloat.values.baudrate); mb->begin(mbFloat.values.baudrate);
  timer = timerBegin(0, 80, true); timerAttachInterrupt(timer, &onTimer, true); timerAlarmWrite(timer, 10000, true); timerAlarmEnable(timer);
  xTaskCreatePinnedToCore(calculate, "calculate", 4000, NULL, 1, NULL, 1); // Core 2
  pinMode(X[0], INPUT); pinMode(X[1], INPUT);
  btn.attach(X[2], INPUT); btn.interval(50); btn.setPressedState(HIGH);
  if(flagCoil[6]) Serial.println("Mulai");
}
void loop() {
  mb->poll();
  esp_task_wdt_reset();
  if(btn.update() && btn.pressed() || flagCoil[4]){
    if(flagCoil[4]) flagCoil[4] = false;
    else if(digitalRead(X[3]) && flagCoil[0]) return; // jika mode manual
    else flagCoil[0] = !flagCoil[0];
    if(flagCoil[0] && !digitalRead(Y[0])){
      flagCoil[2] = true;
      timePrevOn = millis();
      digitalWrite(Y[0], HIGH);
      mbFloat.values.liter = 0.0f;
    }else{
      flagCoil[3] = true;
      timePrevOff = millis();
      digitalWrite(Y[1], LOW);
    }
  }
  if(flagCoil[2] && millis() - timePrevOn >= mbFloat.values.timeInterval_OnValve){
    flagCoil[2] = false;
    digitalWrite(Y[1], HIGH);
    InterruptPinChangeMode(true);
  }
  if(flagCoil[3] && millis() - timePrevOff >= mbFloat.values.timeInterval_OffValve){
    flagCoil[3] = false;
    digitalWrite(Y[0], LOW);
    InterruptPinChangeMode(false);
    flagCoil[1] = true;
  }
}