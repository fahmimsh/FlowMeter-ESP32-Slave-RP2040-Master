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
#include <Nextion.h>

uint8_t idFlow = 1; IPAddress ip(192, 168, 1, 124);
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
    uint16_t timeInterval_OnValve; //12
    uint16_t timeInterval_OffValve; //13
    uint16_t over_fl_err; //14
  } values;
  uint16_t words[15];
};
mbFloatInt mbFloat[2];
/* OnOffFlow, FlagLog, flagOn, flagOff, flagEventOnOff, flagOffPrev, flagPrintserial */
bool flagCoil[2][7], mb_flagCoil[12], IsConnectTCP, IsConnectTCP_Prev;
uint8_t mb_sizeHoldingRegister = sizeof(mbFloat[0].words) / sizeof(mbFloat[0].words[0]) * 2, mb_sizeCoil = 10 + (sizeof(mb_flagCoil) / sizeof(mb_flagCoil[0]));
unsigned long timePrevOn[2], timePrevOff[2], timePrevIsConnectTCP;
//Variable NEXTION
const char *transfer_char[4] = {"Transfer Tank1" , "Transfer Tank2", "Transfer Tank3", "Transfer Tank4"};
const char *sumber_char[2] = {"Sumber Air", "Sumber RO"};
uint8_t index_setval, index_page, index_page_prev, index_page_change;
bool flag_next_set_val, flag_next_read_val;
bool mode_prev[2];
unsigned long time_set_value, time_show_value;
//HALAMAN 1 MAIN
NexCheckbox nex_log[2] = {NexCheckbox(0, 19, "r0"), NexCheckbox(0, 20, "r1")};
NexPage page[3] = {NexPage(0, 0, "page0"), NexPage(2, 0, "page1"), NexPage(1, 0, "KeybdB")};
NexText nex_mode[2] = {NexText(0, 5, "t1"), NexText(0, 14, "t9")};
NexNumber nex_set_liter[2] = {NexNumber (0, 2, "x0"), NexNumber (0, 14, "x4")};
NexNumber nex_liter[2] = {NexNumber (0, 8, "x1"), NexNumber (0, 15, "x5")};
NexNumber nex_FlowRate[2] = {NexNumber (0, 10, "x2"), NexNumber (0, 17, "x6")};
NexNumber nex_factor_k[2] = {NexNumber (0, 21, "x8"), NexNumber (0, 23, "x9")};
NexNumber nex_f_kurang[2] = {NexNumber (0, 27, "x7"), NexNumber (0, 25, "x3")};
NexText nex_sumber[2] = {NexText(0, 29, "t11"), NexText(0, 30, "t14")};
NexText nex_transfer[2] = {NexText(0, 31, "t15"), NexText(0, 32, "t17")};
NexButton nex_btn_setting = NexButton (0, 33, "b0");
//HALAMAN 2 SETTING
NexNumber nex_capacity[2] = {NexNumber (2, 2, "x0"), NexNumber (2, 32, "x5")};
NexNumber nex_over_fl_err[2] = {NexNumber (2, 7, "x8"), NexNumber (2, 30, "x4")};
NexNumber nex_delay_on[2] = {NexNumber (2, 9, "x7"), NexNumber (2, 26, "x2")};
NexNumber nex_delay_off[2] = {NexNumber (2, 14, "x1"), NexNumber (2, 29, "x3")};
NexButton nex_btn_sumber_1[2] = {NexButton (2, 12, "b1"), NexButton (2, 13, "b2")};
NexButton nex_btn_sumber_2[2] = {NexButton (2, 25, "b12"), NexButton (2, 24, "b11")};
NexButton nex_btn_transfer_1[4] = {NexButton (2, 17, "b6"), NexButton (2, 16, "b5"), NexButton (2, 19, "b8"), NexButton (2, 18, "b7")};
NexButton nex_btn_transfer_2[4] = {NexButton (2, 22, "b9"), NexButton (2, 23, "b10"), NexButton (2, 21, "b4"), NexButton (2, 20, "b3")};
NexButton nex_btn_main = NexButton (2, 11, "b0");
//HALAMAN 3 KEYBOARD
NexButton next_btn_ok_keyboard = NexButton (1, 4, "b210");
NexButton next_btn_x_keyboard = NexButton (1, 23, "b251");
//EVENT SET FOR HMI
NexTouch *nex_listen_list[] = {
  &nex_set_liter[0], &nex_set_liter[1],
  &nex_factor_k[0], &nex_factor_k[1],
  &nex_f_kurang[0], &nex_f_kurang[1], 
  &nex_btn_setting,
  &nex_capacity[0], &nex_capacity[1],
  &nex_over_fl_err[0], &nex_over_fl_err[1],
  &nex_delay_on[0], &nex_delay_on[1],
  &nex_delay_off[0], &nex_delay_off[1],
  &nex_btn_sumber_1[0], &nex_btn_sumber_1[1],
  &nex_btn_sumber_2[0], &nex_btn_sumber_2[1],
  &nex_btn_transfer_1[0], &nex_btn_transfer_1[1], &nex_btn_transfer_1[2], &nex_btn_transfer_1[3],
  &nex_btn_transfer_2[0], &nex_btn_transfer_2[1], &nex_btn_transfer_2[2], &nex_btn_transfer_2[3],
  &nex_btn_main,
  &next_btn_ok_keyboard, &next_btn_x_keyboard,
  NULL
};

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
void nex_read_value();
void nex_show_value();
void nex_show_tf_src();
void nex_set_liter_0_event(void *ptr);
void nex_set_liter_1_event(void *ptr);
void nex_factor_k_0_event(void *ptr);
void nex_factor_k_1_event(void *ptr);
void nex_f_kurang_0_event(void *ptr);
void nex_f_kurang_1_event(void *ptr);
void nex_btn_setting_event(void *ptr);
void nex_capacity_0_event(void *ptr);
void nex_capacity_1_event(void *ptr);
void nex_over_fl_err_0_event(void *ptr);
void nex_over_fl_err_1_event(void *ptr);
void nex_delay_on_0_event(void *ptr);
void nex_delay_on_1_event(void *ptr);
void nex_delay_off_0_event(void *ptr);
void nex_delay_off_1_event(void *ptr);
void nex_btn_sumber_1_0_event(void *ptr);
void nex_btn_sumber_1_1_event(void *ptr);
void nex_btn_sumber_2_0_event(void *ptr);
void nex_btn_sumber_2_1_event(void *ptr);
void nex_btn_transfer_1_0_event(void *ptr);
void nex_btn_transfer_1_1_event(void *ptr);
void nex_btn_transfer_1_2_event(void *ptr);
void nex_btn_transfer_1_3_event(void *ptr);
void nex_btn_transfer_2_0_event(void *ptr);
void nex_btn_transfer_2_1_event(void *ptr);
void nex_btn_transfer_2_2_event(void *ptr);
void nex_btn_transfer_2_3_event(void *ptr);
void nex_btn_main_event(void *ptr);
void next_btn_ok_keyboard_event(void *ptr);
void next_btn_x_keyboard_event(void *ptr);
void setup() {
  //nvs_flash_erase(); nvs_flash_init(); while(true); //erase the NVS partition
  Serial.begin(9600);
  prefereces_partition1(true); prefereces_partition2(true);
  mb_flagCoil[0] = mb_flagCoil[4] = mb_flagCoil[6] = mb_flagCoil[10] = true;
  for (int i = 0; i < 8; i++){
    pinMode(X[i], INPUT);
    if(i < 6) { pinMode(Y[i], OUTPUT); digitalWrite(Y[i], HIGH); }
    if(i < 2){
      btn[i].attach(X[i+2], INPUT); btn[i].interval(50); btn[i].setPressedState(HIGH);
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
  nexInit();
  nex_set_liter[0].attachPush(nex_set_liter_0_event, &nex_set_liter[0]);
  nex_set_liter[1].attachPush(nex_set_liter_1_event, &nex_set_liter[1]);
  nex_factor_k[0].attachPush(nex_factor_k_0_event, &nex_factor_k[0]);
  nex_factor_k[1].attachPush(nex_factor_k_1_event, &nex_factor_k[1]);
  nex_f_kurang[0].attachPush(nex_f_kurang_0_event, &nex_f_kurang[0]);
  nex_f_kurang[1].attachPush(nex_f_kurang_1_event, &nex_f_kurang[1]);
  nex_btn_setting.attachPush(nex_btn_setting_event, &nex_btn_setting);
  nex_capacity[0].attachPush(nex_capacity_0_event, &nex_capacity[0]);
  nex_capacity[1].attachPush(nex_capacity_1_event, &nex_capacity[1]);
  nex_over_fl_err[0].attachPush(nex_over_fl_err_0_event, &nex_over_fl_err[0]);
  nex_over_fl_err[1].attachPush(nex_over_fl_err_1_event, &nex_over_fl_err[1]);
  nex_delay_on[0].attachPush(nex_delay_on_0_event, &nex_delay_on[0]);
  nex_delay_on[1].attachPush(nex_delay_on_1_event, &nex_delay_on[1]);
  nex_delay_off[0].attachPush(nex_delay_off_0_event, &nex_delay_off[0]);
  nex_delay_off[1].attachPush(nex_delay_off_1_event, &nex_delay_off[1]);
  nex_btn_sumber_1[0].attachPush(nex_btn_sumber_1_0_event, &nex_btn_sumber_1[0]);
  nex_btn_sumber_1[1].attachPush(nex_btn_sumber_1_1_event, &nex_btn_sumber_1[0]);
  nex_btn_sumber_2[0].attachPush(nex_btn_sumber_2_0_event, &nex_btn_sumber_2[0]);
  nex_btn_sumber_2[1].attachPush(nex_btn_sumber_2_1_event, &nex_btn_sumber_2[1]);
  nex_btn_transfer_1[0].attachPush(nex_btn_transfer_1_0_event, &nex_btn_transfer_1[0]);
  nex_btn_transfer_1[1].attachPush(nex_btn_transfer_1_1_event, &nex_btn_transfer_1[1]);
  nex_btn_transfer_1[2].attachPush(nex_btn_transfer_1_2_event, &nex_btn_transfer_1[2]);
  nex_btn_transfer_1[3].attachPush(nex_btn_transfer_1_3_event, &nex_btn_transfer_1[3]);
  nex_btn_transfer_2[0].attachPush(nex_btn_transfer_2_0_event, &nex_btn_transfer_2[0]);
  nex_btn_transfer_2[1].attachPush(nex_btn_transfer_2_1_event, &nex_btn_transfer_2[1]);
  nex_btn_transfer_2[2].attachPush(nex_btn_transfer_2_2_event, &nex_btn_transfer_2[2]);
  nex_btn_transfer_2[3].attachPush(nex_btn_transfer_2_3_event, &nex_btn_transfer_2[3]);
  nex_btn_main.attachPush(nex_btn_main_event, &nex_btn_main);
  next_btn_ok_keyboard.attachPop(next_btn_ok_keyboard_event, &next_btn_ok_keyboard); 
  next_btn_x_keyboard.attachPop(next_btn_x_keyboard_event, &next_btn_x_keyboard);
  index_page_change = !index_page;
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
  nexLoop(nex_listen_list);
  nex_show_value();
  vTaskDelay(1/portTICK_PERIOD_MS); // Delay 1ms, adjust as needed
}
void btnUpdate(uint8_t index){
 if(btn[index].update() && btn[index].pressed() || flagCoil[index][4]){
    if(flagCoil[index][4]) flagCoil[index][4] = false;
    else if(!digitalRead(X[index+4]) && flagCoil[index][0]) return; // jika mode manual
    else flagCoil[index][0] = !flagCoil[index][0];
    if(flagCoil[0] && digitalRead(Y[index * 2])){
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
  }
  if(flagCoil[index][3] && millis() - timePrevOff[index] >= mbFloat[index].values.timeInterval_OffValve){
    flagCoil[index][3] = false;
    digitalWrite(Y[index * 2], HIGH);
    if(index) InterruptPinChangeMode2(false);
    else InterruptPinChangeMode1(false);
    flagCoil[index][1] = true;
  }
}
void eth_wiz_reset(uint8_t resetPin) { pinMode(resetPin, OUTPUT); digitalWrite(resetPin, HIGH); delay(250); digitalWrite(resetPin, LOW); delay(50); digitalWrite(resetPin, HIGH); delay(350); pinMode(resetPin, INPUT); }
void InterruptPinChangeMode1(bool FlagInit){    
  if(FlagInit){ attachInterrupt(digitalPinToInterrupt(X[0]), plsFL1, RISING); return; }
  detachInterrupt(digitalPinToInterrupt(X[0]));
}
void InterruptPinChangeMode2(bool FlagInit){    
  if(FlagInit){ attachInterrupt(digitalPinToInterrupt(X[1]), plsFL2, RISING); return; }
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
      double _sec = (mils  - prevMillis[0]) / 1000.0f;
      double _freq = pulse[0] / _sec;
      unsigned int _decile = floor(10.0f * _freq / (mbFloat[0].values.capacity * mbFloat[0].values.kFact));
      prevMillis[0] = mils;
      double mFactor[10] = {1,1,1,1,1,1,1,1,1,1};
      unsigned int ceiling =  9; 
      double _Correct = mbFloat[0].values.kFact / mFactor[min(_decile, ceiling)];
      double _Flowrate = _freq / _Correct;
      double _Volume = _Flowrate / (60.0f/_sec);
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
      double _sec = (mils  - prevMillis[1]) / 1000.0f;
      double _freq = pulse[1] / _sec;
      unsigned int _decile = floor(10.0f * _freq / (mbFloat[1].values.capacity * mbFloat[1].values.kFact));
      prevMillis[1] = mils;
      double mFactor[10] = {1,1,1,1,1,1,1,1,1,1};
      unsigned int ceiling =  9; 
      double _Correct = mbFloat[1].values.kFact / mFactor[min(_decile, ceiling)];
      double _Flowrate = _freq / _Correct;
      double _Volume = _Flowrate / (60.0f/_sec);
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
      if(addr_mb_coil < 4) set_mb_flag(addr_mb_coil, 0, 4);
      else if(addr_mb_coil >= 4 && addr_mb_coil < 6) set_mb_flag(addr_mb_coil, 4, 6);
      else if(addr_mb_coil >= 6 && addr_mb_coil < 10) set_mb_flag(addr_mb_coil, 6, 10);
      else if(addr_mb_coil >= 10 && addr_mb_coil < 12) set_mb_flag(addr_mb_coil, 10, 12);
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
        if(index < 4) set_mb_flag(index, 0, 4);
        else if(index >= 4 && index < 6) set_mb_flag(index, 4, 6);
        else if(index >= 6 && index < 10) set_mb_flag(index, 6, 10);
        else if(index >= 10 && index < 12) set_mb_flag(index, 10, 12);
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
    }
   return NMBS_ERROR_NONE;
}
nmbs_error handler_write_single_register(uint16_t address, uint16_t value, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    if(address < 15) {mbFloat[0].words[address] = value; prefereces_partition1(false);}
    else if(address >= 15 && address < 30) {mbFloat[1].words[address - 15] = value; prefereces_partition2(false);}
    return NMBS_ERROR_NONE;
}
nmbs_error handle_write_multiple_registers(uint16_t address, uint16_t quantity, const uint16_t *registers, uint8_t unit_id, void *arg){
    UNUSED_PARAM(arg); UNUSED_PARAM(unit_id);
    if (address + quantity > mb_sizeHoldingRegister + 1)  return NMBS_EXCEPTION_ILLEGAL_DATA_ADDRESS;
    for (int i = 0; i < quantity; i++){
      int8_t index = address + i;
      if(index < 15) {mbFloat[0].words[index] = registers[i]; prefereces_partition1(false);}
      else if(index >= 15 && index < 30) {mbFloat[1].words[index - 15] = registers[i];registers[i]; prefereces_partition2(false);}
    }
    return NMBS_ERROR_NONE;
}
void set_mb_flag(uint8_t index_flag, uint8_t index_start, uint8_t index_stop){
  for (int i = index_start; i < index_stop; i++){
    if(i != index_flag) mb_flagCoil[i] = false;
  }
}
void nex_show_value(){
  if(index_page == 0){
    if(IsConnectTCP_Prev != IsConnectTCP){ IsConnectTCP_Prev = IsConnectTCP; nex_log[0].setValue(IsConnectTCP_Prev); nex_log[1].setValue(IsConnectTCP_Prev); }
    if(mode_prev[0] != digitalRead(X[4])){
      mode_prev[0] = digitalRead(X[4]);
      nex_mode[0].setText(mode_prev[0] ? "Manual" : "Auto" );
    }
    if(mode_prev[1] != digitalRead(X[5])){
      mode_prev[1] = digitalRead(X[5]);
      nex_mode[1].setText(mode_prev[1] ? "Manual" : "Auto" );
    }
  }
  nex_read_value();
  if(millis() - time_show_value >= 1000){
    time_show_value = millis();
    if(index_page == 0){
      nex_liter[0].setValue(mbFloat[0].values.liter * 100); nex_liter[1].setValue(mbFloat[1].values.liter * 100);
      nex_FlowRate[0].setValue(mbFloat[0].values.Flowrate * 100); nex_FlowRate[1].setValue(mbFloat[1].values.Flowrate * 100);
    }
    if(index_page_change != index_page && !flag_next_set_val){
      index_page_change = index_page;
      if(index_page == 0){
        nex_set_liter[0].setValue(mbFloat[0].values.setPoint * 100); nex_set_liter[1].setValue(mbFloat[1].values.setPoint * 100);
        nex_factor_k[0].setValue(mbFloat[0].values.kFact * 10000); nex_factor_k[1].setValue(mbFloat[1].values.kFact * 10000);
        nex_f_kurang[0].setValue(mbFloat[0].values.factorKurang * 100); nex_f_kurang[1].setValue(mbFloat[1].values.factorKurang * 100);
        for (int i = 0; i < 4; i++){
          if(mb_flagCoil[i]) { nex_transfer[0].setText(transfer_char[i]); }
        }
        for (int i = 4; i < 6; i++){
          if(mb_flagCoil[i]) {nex_sumber[0].setText(sumber_char[i - 4]);}
        }
        for (int i = 6; i < 10; i++){
          if(mb_flagCoil[i]) { nex_transfer[1].setText(transfer_char[i - 6]);}
        }
        for (int i = 10; i < 12; i++){
          if(mb_flagCoil[i]) {nex_sumber[1].setText(sumber_char[i - 10]);}
        }
      }
    }
    if(index_page == 1){
      nex_capacity[0].setValue(mbFloat[0].values.capacity * 100);
      nex_capacity[1].setValue(mbFloat[1].values.capacity * 100);
      nex_over_fl_err[0].setValue(mbFloat[0].values.over_fl_err);
      nex_over_fl_err[1].setValue(mbFloat[1].values.over_fl_err);
      nex_delay_on[0].setValue(mbFloat[0].values.timeInterval_OnValve);
      nex_delay_on[1].setValue(mbFloat[1].values.timeInterval_OnValve);
      nex_delay_off[0].setValue(mbFloat[0].values.timeInterval_OffValve);
      nex_delay_off[1].setValue(mbFloat[1].values.timeInterval_OffValve);
      nex_show_tf_src();
    }
  }
}
void nex_read_value(){
  if(millis() - time_set_value >= 1000 && flag_next_read_val){
    flag_next_read_val = false;
    uint32_t value;
    switch (index_setval)
    {
      case 1:
        nex_set_liter[0].getValue(&value);
        mbFloat[0].values.setPoint = (float)value/100;
        break;
      case 2:
        nex_set_liter[1].getValue(&value);
        mbFloat[1].values.setPoint = (float)value/100;
        break;
      case 3:
        nex_factor_k[0].getValue(&value);
        mbFloat[0].values.kFact = (float)value/10000;
        break;
      case 4:
        nex_factor_k[1].getValue(&value);
        mbFloat[1].values.kFact = (float)value/10000;
        break;
      case 5:
        nex_f_kurang[0].getValue(&value);
        mbFloat[0].values.factorKurang = (float)value/100;
        break;
      case 6:
        nex_f_kurang[1].getValue(&value);
        mbFloat[1].values.factorKurang = (float)value/100;
        break;
    }
    flag_next_set_val = false;
    time_show_value = millis();
  }
}
void nex_show_tf_src(){
  if(index_page == 1){
    nex_btn_transfer_1[0].Set_background_color_bco( mb_flagCoil[0] ? 1024:63488);
    nex_btn_transfer_1[1].Set_background_color_bco( mb_flagCoil[1] ? 1024:63488);
    nex_btn_transfer_1[2].Set_background_color_bco( mb_flagCoil[2] ? 1024:63488);
    nex_btn_transfer_1[3].Set_background_color_bco( mb_flagCoil[3] ? 1024:63488);
    nex_btn_sumber_1[0].Set_background_color_bco( mb_flagCoil[4] ? 1024:63488);
    nex_btn_sumber_1[1].Set_background_color_bco( mb_flagCoil[5] ? 1024:63488);
    nex_btn_transfer_2[0].Set_background_color_bco( mb_flagCoil[6] ? 1024:63488);
    nex_btn_transfer_2[1].Set_background_color_bco( mb_flagCoil[7] ? 1024:63488);
    nex_btn_transfer_2[2].Set_background_color_bco( mb_flagCoil[8] ? 1024:63488);
    nex_btn_transfer_2[3].Set_background_color_bco( mb_flagCoil[9] ? 1024:63488);
    nex_btn_sumber_2[0].Set_background_color_bco( mb_flagCoil[10] ? 1024:63488);
    nex_btn_sumber_2[1].Set_background_color_bco( mb_flagCoil[11] ? 1024:63488);
  }
}
void nex_set_liter_0_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 1;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_set_liter_1_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 2;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_factor_k_0_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 3;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_factor_k_1_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 4;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_f_kurang_0_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 5;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_f_kurang_1_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 6;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_btn_setting_event(void *ptr){
  flag_next_set_val = true;
  page[1].show();
  time_show_value = millis();
  flag_next_set_val = false;
  index_page = 1;
}
void nex_capacity_0_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 7;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_capacity_1_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 8;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_over_fl_err_0_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 9;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_over_fl_err_1_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 10;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_delay_on_0_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 11;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_delay_on_1_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 12;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_delay_off_0_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 13;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_delay_off_1_event(void *ptr){
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = 14;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_btn_sumber_1_0_event(void *ptr){
  mb_flagCoil[4] = true; set_mb_flag(4, 4, 6);
  nex_show_tf_src();
}
void nex_btn_sumber_1_1_event(void *ptr){
  mb_flagCoil[5] = true; set_mb_flag(5, 4, 6);
  nex_show_tf_src();
}
void nex_btn_sumber_2_0_event(void *ptr){
  mb_flagCoil[10] = true; set_mb_flag(10, 10, 12);
  nex_show_tf_src();
}
void nex_btn_sumber_2_1_event(void *ptr){
  mb_flagCoil[11] = true; set_mb_flag(11, 10, 12);
  nex_show_tf_src();
}
void nex_btn_transfer_1_0_event(void *ptr){
  mb_flagCoil[0] = true; set_mb_flag(0, 0, 4);
  nex_show_tf_src();
}
void nex_btn_transfer_1_1_event(void *ptr){
  mb_flagCoil[1] = true; set_mb_flag(1, 0, 4);
  nex_show_tf_src();
}
void nex_btn_transfer_1_2_event(void *ptr){
  mb_flagCoil[2] = true; set_mb_flag(2, 0, 4);
  nex_show_tf_src();
}
void nex_btn_transfer_1_3_event(void *ptr){
  mb_flagCoil[3] = true; set_mb_flag(3, 0, 4);
  nex_show_tf_src();
}
void nex_btn_transfer_2_0_event(void *ptr){
  mb_flagCoil[6] = true; set_mb_flag(6, 6, 10);
  nex_show_tf_src();
}
void nex_btn_transfer_2_1_event(void *ptr){
  mb_flagCoil[7] = true; set_mb_flag(7, 6, 10);
  nex_show_tf_src();
}
void nex_btn_transfer_2_2_event(void *ptr){
  mb_flagCoil[8] = true; set_mb_flag(8, 6, 10);
  nex_show_tf_src();
}
void nex_btn_transfer_2_3_event(void *ptr){
  mb_flagCoil[9] = true; set_mb_flag(9, 6, 10);
  nex_show_tf_src();
}
void nex_btn_main_event(void *ptr){
  flag_next_set_val = true;

  page[0].show();
  time_show_value = millis();
  flag_next_set_val = false;
  index_page = 0;
}
void next_btn_ok_keyboard_event(void *ptr){
  flag_next_set_val = true;
  index_page = index_page_prev;

  time_set_value = millis();
  flag_next_read_val = true;
}
void next_btn_x_keyboard_event(void *ptr){
  flag_next_set_val = true;
  index_page = index_page_prev;
  index_setval = 0;
  flag_next_set_val = false;
  time_show_value = millis();
}