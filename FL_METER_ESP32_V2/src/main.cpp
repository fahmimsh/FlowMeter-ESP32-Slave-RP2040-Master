#include <Arduino.h>
#include <SPI.h>
#include <WiFi.h>
#include <Ethernet2.h>
#include <nanomodbus.h>
#include <Bounce2.h>
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <esp_task_wdt.h>
#include <Preferences.h>
#include <nvs_flash.h>
#include <time.h>
#include <TimeLib.h>

uint8_t idFlow = 1; IPAddress ip(192, 168, 1, 104);
IPAddress gateway(192, 168, 1, 1), dns_server(192, 168, 110, 201), subnet(255,255,255,0);

const int X[8] = {26, 25, 33, 32, 35, 34, 39, 36}, Y[6] = {14, 13, 15, 2, 4, 0};
volatile int count[2], pulse[2];
volatile bool flagTime[2];
unsigned long prevMillis[2];
union mbFloatInt {
  struct {
    float kFact; //0, 1
    float capacity; //2, 3
    float setPoint; //4, 5
    float factorKurang; // 6, 7
    float liter; //8, 9
    float Flowrate; //10, 11
    float Freq; //12 13
    uint16_t timeInterval_OnValve; //14
    uint16_t timeInterval_OffValve; //15
    uint16_t over_fl_err; //16
  } values;
  uint16_t words[16];
};
mbFloatInt mbFloat[2];
union mb_Int_Date_Time {
  struct {
    uint16_t day; //17
    uint16_t month; //18
    uint16_t year; //19
    uint16_t hour; //20
    uint16_t minute; //21
    uint16_t second; //22
  } values;
  uint16_t words[7];
};
mb_Int_Date_Time mb_int_date_time;
/* OnOffFlow, FlagLog, flagOn, flagOff, flagEventOnOff, flagOffPrev, flagPrintserial */
bool flagCoil[2][7], mb_flagCoil[29], IsConnectTCP, IsConnectTCP_Prev;
bool flag_prev_log[2];
bool mode_produksi_persiapan[2];
uint8_t mb_sizeHoldingRegister = (sizeof(mbFloat[0].words) / sizeof(mbFloat[0].words[0]) * 2) + sizeof(mb_int_date_time.words) / sizeof(mb_int_date_time.words[0]), mb_sizeCoil = 10 + (sizeof(mb_flagCoil) / sizeof(mb_flagCoil[0]));
unsigned long timePrevOn[2], timePrevOff[2], timePrevIsConnectTCP;
bool mode_prev[2];

Bounce2::Button btn[2];
Preferences prefs;
EthernetServer server(502);
EthernetClient client;
nmbs_t nmbsTCP; nmbs_error errTCP;
#define UNUSED_PARAM(x) ((void)(x))
hw_timer_t *timer0 = NULL, *timer1 = NULL;
portMUX_TYPE synch0 = portMUX_INITIALIZER_UNLOCKED, synch1 = portMUX_INITIALIZER_UNLOCKED;
portMUX_TYPE timerMux0 = portMUX_INITIALIZER_UNLOCKED, timerMux1 = portMUX_INITIALIZER_UNLOCKED;
void IRAM_ATTR plsFL1() { portENTER_CRITICAL(&synch0); count[0]++; portEXIT_CRITICAL(&synch0);}
void IRAM_ATTR plsFL2() { portENTER_CRITICAL(&synch1); count[1]++; portEXIT_CRITICAL(&synch1);}
void IRAM_ATTR onTimer1() { portENTER_CRITICAL_ISR(&timerMux0); flagTime[0] = true; portEXIT_CRITICAL_ISR(&timerMux0);}
void IRAM_ATTR onTimer2() { portENTER_CRITICAL_ISR(&timerMux1); flagTime[1] = true; portEXIT_CRITICAL_ISR(&timerMux1);}
void InterruptPinChangeMode1(bool FlagInit);
void InterruptPinChangeMode2(bool FlagInit);
float formulaLiter1(double &volume), formulaLiter2(double &volume);
void calculate1(void *pvParameters);
void calculate2(void *pvParameters);
void eth_wiz_reset(uint8_t resetPin);
void prefereces_partition1(bool ReadOrWrite);
void prefereces_partition2(bool ReadOrWrite);
void prefereces_partition_date_time(bool ReadOrWrite);
void btnUpdate(uint8_t index);
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
void set_mb_flag(uint8_t index_flag, uint8_t index_start, uint8_t index_stop);

void setup() {
  //nvs_flash_erase(); nvs_flash_init(); while(true); //erase the NVS partition
  Serial.begin(9600);
  prefereces_partition1(true); vTaskDelay(1/portTICK_PERIOD_MS);
  prefereces_partition2(true); vTaskDelay(1/portTICK_PERIOD_MS);
  prefereces_partition_date_time(true); vTaskDelay(1/portTICK_PERIOD_MS);
  mb_flagCoil[0] = mb_flagCoil[5] = mb_flagCoil[9] = mb_flagCoil[19] = true;
  IsConnectTCP_Prev = !IsConnectTCP;
  for (int i = 0; i < 8; i++){
    pinMode(X[i], INPUT);
    if(i < 6) { pinMode(Y[i], OUTPUT); digitalWrite(Y[i], HIGH); }
    if(i < 2){
      btn[i].attach(X[i+2], INPUT); btn[i].interval(70); btn[i].setPressedState(HIGH);
    }
  }
  esp_task_wdt_init(75, true); esp_task_wdt_add(NULL);
  byte* _mac = new byte[6]; WiFi.macAddress(_mac);
  eth_wiz_reset(27);
  Ethernet.init(5);
  Ethernet.begin(_mac, ip,dns_server,gateway,subnet);
  server.begin();
  nmbs_platform_conf platform_confTCP;
  platform_confTCP.transport = NMBS_TRANSPORT_TCP;
  platform_confTCP.read = read_ethernet;
  platform_confTCP.write = write_ethernet;
  platform_confTCP.arg = NULL;    // We will set the arg (socket fd) later
  nmbs_callbacks callbacks = {0};
  callbacks.read_discrete_inputs = handle_read_discrete_inputs;
  callbacks.read_coils = handle_read_coils;
  callbacks.write_single_coil = hadle_write_single_coils;
  callbacks.write_multiple_coils = handle_write_multiple_coils;
  callbacks.read_holding_registers = handler_read_holding_registers;
  callbacks.write_single_register = handler_write_single_register;
  callbacks.write_multiple_registers = handle_write_multiple_registers;
  errTCP = nmbs_server_create(&nmbsTCP, 1, &platform_confTCP, &callbacks);
  if (errTCP != NMBS_ERROR_NONE) Serial.printf("Error on modbus connection TCP - %s\n", nmbs_strerror(errTCP));
  nmbs_set_read_timeout(&nmbsTCP, 1000);
  nmbs_set_byte_timeout(&nmbsTCP, 1000);
  setTime(mb_int_date_time.values.hour, mb_int_date_time.values.minute, mb_int_date_time.values.second, mb_int_date_time.values.day, mb_int_date_time.values.month, mb_int_date_time.values.year); // --> setTime(hr,min,sec,day,mnth,yr);
  timer0 = timerBegin(0, 80, true); timerAttachInterrupt(timer0, &onTimer1, true); timerAlarmWrite(timer0, 10000, true);
  timer1 = timerBegin(1, 80, true); timerAttachInterrupt(timer1, &onTimer2, true); timerAlarmWrite(timer1, 10000, true);
  timerAlarmEnable(timer0); timerAlarmEnable(timer1);
  xTaskCreatePinnedToCore(calculate1, "calculate1", 4000, NULL, 1, NULL, 1); // Core 2
  xTaskCreatePinnedToCore(calculate2, "calculate2", 4000, NULL, 1, NULL, 1); // Core 2
  Serial.println("Mulai");
}

void loop() {
  // put your main code here, to run repeatedly:
  esp_task_wdt_reset();
  btnUpdate(0); btnUpdate(1);
  if(IsConnectTCP && millis() - timePrevIsConnectTCP >= 5000) IsConnectTCP = false;
  mbTCPpoll();
  vTaskDelay(1/portTICK_PERIOD_MS); // Delay 1ms, adjust as needed
}
void btnUpdate(uint8_t index){
 if(btn[index].update() && btn[index].pressed() || flagCoil[index][4]){
    if(flagCoil[index][4]) flagCoil[index][4] = false;
    else if(!digitalRead(X[index+4]) && flagCoil[index][0]) return; // jika mode manual
    else flagCoil[index][0] = !flagCoil[index][0];
    if(flagCoil[index][0] && digitalRead(Y[index * 2])){
      flagCoil[index][2] = true;
      timePrevOn[index] = millis();
      digitalWrite(Y[index * 2], LOW);
      mbFloat[index].values.liter = 0.0f;
    }else{
      flagCoil[index][3] = true;
      timePrevOff[index] = millis();
      digitalWrite(Y[(index * 2) + 1], HIGH);
    }
  }
  if(flagCoil[index][2] && millis() - timePrevOn[index] >= mbFloat[index].values.timeInterval_OnValve){
    flagCoil[index][2] = false;
    digitalWrite(Y[(index * 2) + 1], LOW);
    if(index) InterruptPinChangeMode2(true);
    else InterruptPinChangeMode1(true);
    flagCoil[index][1] = false;
  }
  if(flagCoil[index][3] && millis() - timePrevOff[index] >= mbFloat[index].values.timeInterval_OffValve){
    flagCoil[index][3] = false;
    digitalWrite(Y[index * 2], HIGH);
    if(index) InterruptPinChangeMode2(false);
    else InterruptPinChangeMode1(false);
    flagCoil[index][1] = mode_produksi_persiapan[index] ? false : true;
  }
}
void eth_wiz_reset(uint8_t resetPin) { pinMode(resetPin, OUTPUT); digitalWrite(resetPin, HIGH); delay(250); digitalWrite(resetPin, LOW); delay(50); digitalWrite(resetPin, HIGH); delay(350); pinMode(resetPin, INPUT); }
void InterruptPinChangeMode1(bool FlagInit){    
  if(FlagInit){ attachInterrupt(digitalPinToInterrupt(X[0]), plsFL1, FALLING); return; }
  detachInterrupt(digitalPinToInterrupt(X[0]));
}
void InterruptPinChangeMode2(bool FlagInit){    
  if(FlagInit){ attachInterrupt(digitalPinToInterrupt(X[1]), plsFL2, FALLING); return; }
  detachInterrupt(digitalPinToInterrupt(X[1]));
}
float formulaLiter1(double &volume){
  float setPoint_fl = mbFloat[0].values.setPoint - mbFloat[0].values.factorKurang;
  if(!digitalRead(Y[0]) && mbFloat[0].values.liter >= setPoint_fl && !digitalRead(X[4])){
    static float target_err_over_fl = 0.0f;
    if(flagCoil[0][0]){
      flagCoil[0][0] = false;
      flagCoil[0][4] = true;
      mbFloat[0].values.liter += volume;
      target_err_over_fl = mbFloat[0].values.liter - mbFloat[0].values.setPoint;
    }
    return mbFloat[0].values.liter + abs(volume/(static_cast<float>(mbFloat[0].values.over_fl_err)/10.0f));
  } else return mbFloat[0].values.liter + volume;
}
float formulaLiter2(double &volume){
  float setPoint_fl = mbFloat[1].values.setPoint - mbFloat[1].values.factorKurang;
  if(!digitalRead(Y[2]) && mbFloat[1].values.liter >= setPoint_fl && !digitalRead(X[5])){
    static float target_err_over_fl = 0.0f;
    if(flagCoil[1][0]){
      flagCoil[1][0] = false;
      flagCoil[1][4] = true;
      mbFloat[1].values.liter += volume;
      target_err_over_fl = mbFloat[1].values.liter - mbFloat[1].values.setPoint;
    }
    return mbFloat[1].values.liter + abs(volume/(static_cast<float>(mbFloat[1].values.over_fl_err)/10.0f));
  } else return mbFloat[1].values.liter + volume;
}
void calculate1(void *pvParameters) {
  (void)pvParameters;
  for (;;) {
    if(flagTime[0]){
      portENTER_CRITICAL_ISR(&timerMux0); flagTime[0] = false; portEXIT_CRITICAL_ISR(&timerMux0);
      portENTER_CRITICAL(&synch0); pulse[0] = count[0]; count[0] = 0; portEXIT_CRITICAL(&synch0);
      unsigned long mils = millis(); 
      double _sec = (mils  - prevMillis[0]) / 1000.0;
      double _freq = pulse[0] / _sec;
      unsigned int _decile = floor(10.0f * _freq / (mbFloat[0].values.capacity * mbFloat[0].values.kFact));
      prevMillis[0] = mils;
      double mFactor[10] = {1,1,1,1,1,1,1,1,1,1};
      unsigned int ceiling =  9; 
      double _Correct = mbFloat[0].values.kFact / mFactor[min(_decile, ceiling)];
      double _Flowrate = _freq / _Correct;
      double _Volume = _Flowrate / (60.0/_sec);
      mbFloat[0].values.Flowrate = _Flowrate;
      mbFloat[0].values.liter = formulaLiter1(_Volume);
      
    }
    vTaskDelay(1/portTICK_PERIOD_MS); // Delay 1ms, adjust as needed
  }
}
void calculate2(void *pvParameters) {
  (void)pvParameters;
  for (;;) {
    if(flagTime[1]){
      portENTER_CRITICAL_ISR(&timerMux1); flagTime[1] = false; portEXIT_CRITICAL_ISR(&timerMux1);
      portENTER_CRITICAL(&synch1); pulse[1] = count[1]; count[1] = 0; portEXIT_CRITICAL(&synch1);
      unsigned long mils = millis(); 
      double _sec = (mils  - prevMillis[1]) / 1000.0;
      double _freq = pulse[1] / _sec;
      unsigned int _decile = floor(10.0f * _freq / (mbFloat[1].values.capacity * mbFloat[1].values.kFact));
      prevMillis[1] = mils;
      double mFactor[10] = {1,1,1,1,1,1,1,1,1,1};
      unsigned int ceiling =  9; 
      double _Correct = mbFloat[1].values.kFact / mFactor[min(_decile, ceiling)];
      double _Flowrate = _freq / _Correct;
      double _Volume = _Flowrate / (60.0/_sec);
      mbFloat[1].values.Flowrate = _Flowrate;
      mbFloat[1].values.liter = formulaLiter2(_Volume);
      
    }
    vTaskDelay(1/portTICK_PERIOD_MS); // Delay 1ms, adjust as needed
  }
}
void prefereces_partition1(bool ReadOrWrite){
  prefs.begin("file-app", false);
  if(ReadOrWrite){
    mbFloat[0].values.kFact = prefs.getFloat("kFact0", 500.0f);
    mbFloat[0].values.capacity = prefs.getFloat("capacity0", 500.0f);
    mbFloat[0].values.setPoint = prefs.getFloat("setPoint0", 100.0f);
    mbFloat[0].values.factorKurang = prefs.getFloat("factorKurang0", 0.8f);
    mbFloat[0].values.timeInterval_OnValve = prefs.getUInt("time-OnValve0", 200);
    mbFloat[0].values.timeInterval_OffValve = prefs.getUInt("time-OffValve0", 200);
    mbFloat[0].values.over_fl_err = prefs.getUInt("over_fl_err0", 10);
  }else{
    prefs.putFloat("kFact0", mbFloat[0].values.kFact);
    prefs.putFloat("capacity0", mbFloat[0].values.capacity);
    prefs.putFloat("setPoint0", mbFloat[0].values.setPoint);
    prefs.putFloat("factorKurang0", mbFloat[0].values.factorKurang);
    prefs.putUInt("time-OnValve0", mbFloat[0].values.timeInterval_OnValve);
    prefs.putUInt("time-OffValve0", mbFloat[0].values.timeInterval_OffValve);
    prefs.putUInt("over_fl_err0", mbFloat[0].values.over_fl_err);
  }
  prefs.end();
}
void prefereces_partition2(bool ReadOrWrite){
  prefs.begin("file-app", false);
  if(ReadOrWrite){
    mbFloat[1].values.kFact = prefs.getFloat("kFact1", 500.0f);
    mbFloat[1].values.capacity = prefs.getFloat("capacity1", 500.0f);
    mbFloat[1].values.setPoint = prefs.getFloat("setPoint1", 100.0f);
    mbFloat[1].values.factorKurang = prefs.getFloat("factorKurang1", 0.8f);
    mbFloat[1].values.timeInterval_OnValve = prefs.getUInt("time-OnValve1", 200);
    mbFloat[1].values.timeInterval_OffValve = prefs.getUInt("time-OffValve1", 200);
    mbFloat[1].values.over_fl_err = prefs.getUInt("over_fl_err1", 10);
  }else{
    prefs.putFloat("kFact1", mbFloat[1].values.kFact);
    prefs.putFloat("capacity1", mbFloat[1].values.capacity);
    prefs.putFloat("setPoint1", mbFloat[1].values.setPoint);
    prefs.putFloat("factorKurang1", mbFloat[1].values.factorKurang);
    prefs.putUInt("time-OnValve1", mbFloat[1].values.timeInterval_OnValve);
    prefs.putUInt("time-OffValve1", mbFloat[1].values.timeInterval_OffValve);
    prefs.putUInt("over_fl_err1", mbFloat[1].values.over_fl_err);
  }
  prefs.end();
}
void prefereces_partition_date_time(bool ReadOrWrite){
  prefs.begin("file-app", false);
  if(ReadOrWrite){
    mb_int_date_time.values.day = prefs.getUInt("hari", 1);
    mb_int_date_time.values.month = prefs.getUInt("bulan", 1);
    mb_int_date_time.values.year = prefs.getUInt("tahun", 2024);
    mb_int_date_time.values.hour = prefs.getUInt("jam", 0);
    mb_int_date_time.values.minute = prefs.getUInt("menit", 0);
    mb_int_date_time.values.second = prefs.getUInt("detik", 0);
  }else{
    prefs.putUInt("hari", mb_int_date_time.values.day);
    prefs.putUInt("bulan", mb_int_date_time.values.month);
    prefs.putUInt("tahun", mb_int_date_time.values.year);
    prefs.putUInt("jam", mb_int_date_time.values.hour);
    prefs.putUInt("menit", mb_int_date_time.values.minute);
    prefs.putUInt("detik", mb_int_date_time.values.second);
  }
  prefs.end();
}
int32_t read_ethernet(uint8_t* buf, uint16_t count, int32_t timeout_ms, void* arg) {
  client.setTimeout(timeout_ms); return client.readBytes(buf, count);
}
int32_t write_ethernet(const uint8_t* buf, uint16_t count, int32_t timeout_ms, void* arg) {
  client.setTimeout(timeout_ms); return client.write(buf, count);
}
void mbTCPpoll(){
  client = server.available();
  if (client && client.connected()){
    if(client.available()){
      errTCP = nmbs_server_poll(&nmbsTCP);
      timePrevIsConnectTCP = millis(); if(!IsConnectTCP) IsConnectTCP = true;
      if (errTCP != NMBS_ERROR_NONE) Serial.printf("Error on modbus TCP - %s\n", nmbs_strerror(errTCP));
      else client.flush();
    }
  }else client.stop();
}
nmbs_error handle_read_discrete_inputs(uint16_t address, uint16_t quantity, uint8_t *inputs_out, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > 6) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++) {
        uint8_t index = address + i;
        if(index < 4) nmbs_bitfield_write(inputs_out, i, !digitalRead(X[index + 4]));
        else nmbs_bitfield_write(inputs_out, i, IsConnectTCP);
    }
    return NMBS_ERROR_NONE;
}
nmbs_error handle_read_coils(uint16_t address, uint16_t quantity, nmbs_bitfield coils_out, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeCoil + 1) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++) {
      uint8_t index = address + i;
      if(index < 6) nmbs_bitfield_write(coils_out, i, !digitalRead(Y[index]));
      else if(index >= 6 && index < 8) nmbs_bitfield_write(coils_out, i, flagCoil[0][index - 6]);
      else if(index < 10 && index >= 8) nmbs_bitfield_write(coils_out, i, flagCoil[1][index - 8]);
      else nmbs_bitfield_write(coils_out, i, mb_flagCoil[index - 10]);
    }
    return NMBS_ERROR_NONE;
}
nmbs_error hadle_write_single_coils(uint16_t address, bool value, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address > mb_sizeCoil) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    if(address < 6) digitalWrite(Y[address], !value);
    else if(address >= 6 && address < 8){
      flagCoil[0][address - 6] = value;
      if(address == 6){
        if(!digitalRead(X[4]) && !digitalRead(Y[0])){
          if(!flagCoil[0][0]) flagCoil[0][0] = !flagCoil[0][0];
        }else { flagCoil[0][4] = true; }
      }
    }
    else if(address < 10 && address >= 8){
      flagCoil[1][address - 8] = value;
      if(address == 8){
        if(!digitalRead(X[5]) && !digitalRead(Y[2])){
          if(!flagCoil[1][0]) flagCoil[1][0] = !flagCoil[1][0];
        }else { flagCoil[1][4] = true; }
      }
    }
    else{
      uint8_t addr_mb_coil = address - 10;
      mb_flagCoil[address - 10] = value;
      if(addr_mb_coil < 5) set_mb_flag(addr_mb_coil, 0, 5);
      else if(addr_mb_coil >= 5 && addr_mb_coil < 9) set_mb_flag(addr_mb_coil, 5, 9);
      else if(addr_mb_coil >= 9 && addr_mb_coil < 19) set_mb_flag(addr_mb_coil, 9, 19);
      else if(addr_mb_coil >= 19 && addr_mb_coil < 29) set_mb_flag(addr_mb_coil, 19, 29);
    }
    return NMBS_ERROR_NONE;
}
nmbs_error handle_write_multiple_coils(uint16_t address, uint16_t quantity, const uint8_t *coils, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeCoil + 1) return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++){
      uint8_t index = address + i;
      if(index < 6) digitalWrite(Y[address], !nmbs_bitfield_read(coils, i));
      else if(index >= 6 && index < 8){
        flagCoil[0][index - 6] = nmbs_bitfield_read(coils, i);
        if(index == 6){
          if(!digitalRead(X[4]) && !digitalRead(Y[0])){
            if(!flagCoil[0][0]) flagCoil[0][0] = !flagCoil[0][0];
          }else flagCoil[0][4] = true;
        }
      }
      else if(index < 10 && index >= 8){
        flagCoil[1][index - 8] = nmbs_bitfield_read(coils, i);
        if(index == 8){
          if(!digitalRead(X[5]) && !digitalRead(Y[2])){
            if(!flagCoil[1][0]) flagCoil[1][0] = !flagCoil[1][0];
          }else flagCoil[1][4] = true;
        }
      }
      else{
        mb_flagCoil[index] = nmbs_bitfield_read(coils, i);
        if(index < 5) set_mb_flag(index, 0, 5);
        else if(index >= 5 && index < 9) set_mb_flag(index, 5, 9);
        else if(index >= 9 && index < 19) set_mb_flag(index, 9, 19);
        else if(index >= 19 && index < 29) set_mb_flag(index, 19, 29);
      }
    }
    return NMBS_ERROR_NONE;
}
nmbs_error handler_read_holding_registers(uint16_t address, uint16_t quantity, uint16_t *registers_out, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++){
      uint8_t index = address + i;
      if(index < 15) registers_out[i] = mbFloat[0].words[index];
      else if(index >= 15 && index < 30) registers_out[i] = mbFloat[1].words[index - 15];
      else if(index >= 30 && index < 37) registers_out[i] = mb_int_date_time.words[index - 30];
    }
   return NMBS_ERROR_NONE;
}
nmbs_error handler_write_single_register(uint16_t address, uint16_t value, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    if(address < 15) {mbFloat[0].words[address] = value; prefereces_partition1(false);}
    else if(address >= 15 && address < 30) {mbFloat[1].words[address - 15] = value; prefereces_partition2(false);}
    else if(address >= 30 && address < 37) {mb_int_date_time.words[address - 30] = value; prefereces_partition_date_time(false); setTime(mb_int_date_time.values.hour, mb_int_date_time.values.minute, mb_int_date_time.values.second, mb_int_date_time.values.day, mb_int_date_time.values.month, mb_int_date_time.values.year); }
    return NMBS_ERROR_NONE;
}
nmbs_error handle_write_multiple_registers(uint16_t address, uint16_t quantity, const uint16_t *registers, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++){
      int8_t index = address + i;
      if(index < 15) {mbFloat[0].words[index] = registers[i]; prefereces_partition1(false);}
      else if(index >= 15 && index < 30) {mbFloat[1].words[index - 15] = registers[i]; prefereces_partition2(false);}
      else if(index >= 30 && index < 37) {mb_int_date_time.words[index - 30] = registers[i]; prefereces_partition_date_time(false); setTime(mb_int_date_time.values.hour, mb_int_date_time.values.minute, mb_int_date_time.values.second, mb_int_date_time.values.day, mb_int_date_time.values.month, mb_int_date_time.values.year); }
    }
    return NMBS_ERROR_NONE;
}
void set_mb_flag(uint8_t index_flag, uint8_t index_start, uint8_t index_stop){
  for (int i = index_start; i < index_stop; i++){
    if(i != index_flag) mb_flagCoil[i] = false;
  }
}