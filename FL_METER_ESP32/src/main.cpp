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
#include <time.h>
#include <TimeLib.h>

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
  uint16_t words[16];
};
mbFloatInt mbFloat[2];
union mb_Int_Date_Time {
  struct {
    uint16_t day; //15
    uint16_t month; //16
    uint16_t year; //17
    uint16_t hour; //18
    uint16_t minute; //19
    uint16_t second; //20
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
//Variable NEXTION
const char *tanki_1_char[5] = {"Tanki T1" , "Tanki T2", "Tanki T3", "Tanki T4", "Tanki T5"};
const char *tanki_2_char[4] = {"Tanki A1" , "Tanki A2", "Tanki B1", "Tanki B2"};
const char *batch_char[10] = {"Batch 01", "Batch 02", "Batch 03", "Batch 04", "Batch 05", "Batch 06", "Batch 07", "Batch 08", "Batch 09", "Batch 10"};
uint8_t index_setval, index_page, index_page_prev, index_page_change;
bool flag_next_set_val, flag_next_read_val, flag_next_setvalue;
bool mode_prev[2];
unsigned long time_set_value, time_show_value;
//ALL HALAMAN
NexPage page[4] = {NexPage(0, 0, "page0"), NexPage(2, 0, "page1"), NexPage(1, 0, "KeybdB"), NexPage(3, 0, "page2")};
//HALAMAN 1 MAIN
NexText nex_com_conn = NexText(0, 36, "t15");
NexText nex_date_time = NexText(0, 30, "t18");
NexCheckbox nex_log[2] = {NexCheckbox(0, 19, "r0"), NexCheckbox(0, 20, "r1")};
NexText nex_mode[2] = {NexText(0, 5, "t1"), NexText(0, 12, "t9")};
NexNumber nex_set_liter[2] = {NexNumber (0, 2, "x0"), NexNumber (0, 14, "x4")};
NexNumber nex_liter[2] = {NexNumber (0, 8, "x1"), NexNumber (0, 15, "x5")};
NexNumber nex_FlowRate[2] = {NexNumber (0, 10, "x2"), NexNumber (0, 17, "x6")};
NexNumber nex_factor_k[2] = {NexNumber (0, 21, "x8"), NexNumber (0, 23, "x9")};
NexNumber nex_f_kurang[2] = {NexNumber (0, 27, "x7"), NexNumber (0, 25, "x3")};
NexButton nex_btn_tank1 = NexButton (0, 29, "b0");
NexButton nex_btn_tank2 = NexButton (0, 35, "b3");
NexButton nex_btn_batch1 = NexButton (0, 31, "b1");
NexButton nex_btn_batch2 = NexButton (0, 34, "b2");
NexButton nex_btn_mode_produksi1 = NexButton (0, 37, "b4");
NexButton nex_btn_mode_produksi2 = NexButton (0, 38, "b5");
//HALAMAN 2 SETTING 1
NexNumber nex_capacity[2] = {NexNumber (2, 2, "x0"), NexNumber (2, 28, "x5")};
NexNumber nex_over_fl_err[2] = {NexNumber (2, 7, "x8"), NexNumber (2, 26, "x4")};
NexNumber nex_delay_on[2] = {NexNumber (2, 9, "x7"), NexNumber (2, 22, "x2")};
NexNumber nex_delay_off[2] = {NexNumber (2, 12, "x1"), NexNumber (2, 25, "x3")};
NexButton nex_btn_set_tank1[5] = {NexButton (2, 15, "b6"), NexButton (2, 14, "b5"), NexButton (2, 17, "b8"), NexButton (2, 16, "b7"), NexButton (2, 32, "b1")};
NexButton nex_btn_set_tank2[4] = {NexButton (2, 20, "b9"), NexButton (2, 21, "b10"), NexButton (2, 19, "b4"), NexButton (2, 18, "b3")};
NexButton nex_btn_main = NexButton (2, 11, "b0");
//HALAMAN 3 KEYBOARD
NexButton next_btn_ok_keyboard = NexButton (1, 4, "b210");
NexButton next_btn_x_keyboard = NexButton (1, 23, "b251");
//HALAMAN 4 SETTING 2
NexButton next_btn_set_batch1[10] {
  NexButton (3, 15, "b2"), NexButton (3, 16, "b11"), NexButton (3, 22, "b17"), NexButton (3, 21, "b16"), NexButton (3, 7, "b6"), NexButton (3, 6, "b5"), NexButton (3, 9, "b8"), NexButton (3, 8, "b7"), NexButton (3, 14, "b1"), NexButton (3, 23, "b18")
};
NexButton next_btn_set_batch2[10] {
  NexButton (3, 17, "b12"), NexButton (3, 18, "b13"), NexButton (3, 20, "b15"), NexButton (3, 19, "b14"), NexButton (3, 12, "b9"), NexButton (3, 13, "b10"), NexButton (3, 11, "b4"), NexButton (3, 10, "b3"), NexButton (3, 25, "b20"), NexButton (3, 24, "b19")
};
NexButton nex_btn_ok = NexButton (3, 5, "b0");
//EVENT SET FOR HMI
NexTouch *nex_listen_list[] = {
  &nex_set_liter[0], &nex_set_liter[1],
  &nex_factor_k[0], &nex_factor_k[1],
  &nex_f_kurang[0], &nex_f_kurang[1],

  &nex_btn_tank1, &nex_btn_tank2, &nex_btn_batch1, &nex_btn_batch2, &nex_btn_mode_produksi1, &nex_btn_mode_produksi2,

  &nex_capacity[0], &nex_capacity[1],
  &nex_over_fl_err[0], &nex_over_fl_err[1],
  &nex_delay_on[0], &nex_delay_on[1],
  &nex_delay_off[0], &nex_delay_off[1],

  &nex_btn_set_tank1[0], &nex_btn_set_tank1[1], &nex_btn_set_tank1[2], &nex_btn_set_tank1[3], &nex_btn_set_tank1[4],
  &nex_btn_set_tank2[0], &nex_btn_set_tank2[1], &nex_btn_set_tank2[2], &nex_btn_set_tank2[3],
  &nex_btn_main,

  &next_btn_set_batch1[0], &next_btn_set_batch1[1], &next_btn_set_batch1[2], &next_btn_set_batch1[3], &next_btn_set_batch1[4], &next_btn_set_batch1[5], &next_btn_set_batch1[6], &next_btn_set_batch1[7], &next_btn_set_batch1[8], &next_btn_set_batch1[9],
  &next_btn_set_batch2[0], &next_btn_set_batch2[1], &next_btn_set_batch2[2], &next_btn_set_batch2[3], &next_btn_set_batch2[4], &next_btn_set_batch2[5], &next_btn_set_batch2[6], &next_btn_set_batch2[7], &next_btn_set_batch2[8], &next_btn_set_batch2[9],
  &nex_btn_ok,

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
void nex_show_date_time();
void nex_read_value();
void nex_show_value();
void nex_show_set_and_tank();
void nex_show_batch();
void nex_init_event_all();
void nex_set_value_for_event(void *ptr, int index_setval_);
void nex_btn_set_tank_event(void *ptr, int index, int start_index, int end_index);
void nex_btn_set_batch_event(void *ptr, int index, int start_index, int end_index);
void nex_btn_set_all_event(void *ptr, int page_index);

void nex_set_liter_0_event(void *ptr);
void nex_set_liter_1_event(void *ptr);
void nex_factor_k_0_event(void *ptr);
void nex_factor_k_1_event(void *ptr);
void nex_f_kurang_0_event(void *ptr);
void nex_f_kurang_1_event(void *ptr);
void nex_btn_tank1_event(void *ptr);
void nex_btn_tank2_event(void *ptr);
void nex_btn_batch1_event(void *ptr);
void nex_btn_batch2_event(void *ptr);
void nex_btn_mode_produksi1_event(void *ptr);
void nex_btn_mode_produksi2_event(void *ptr);

void nex_capacity_0_event(void *ptr);
void nex_capacity_1_event(void *ptr);
void nex_over_fl_err_0_event(void *ptr);
void nex_over_fl_err_1_event(void *ptr);
void nex_delay_on_0_event(void *ptr);
void nex_delay_on_1_event(void *ptr);
void nex_delay_off_0_event(void *ptr);
void nex_delay_off_1_event(void *ptr);

void nex_btn_set_tank1_0_event(void *ptr);
void nex_btn_set_tank1_1_event(void *ptr);
void nex_btn_set_tank1_2_event(void *ptr);
void nex_btn_set_tank1_3_event(void *ptr);
void nex_btn_set_tank1_4_event(void *ptr);

void nex_btn_set_tank2_0_event(void *ptr);
void nex_btn_set_tank2_1_event(void *ptr);
void nex_btn_set_tank2_2_event(void *ptr);
void nex_btn_set_tank2_3_event(void *ptr);
void nex_btn_main_event(void *ptr);

void next_btn_set_batch1_0_event(void *ptr);
void next_btn_set_batch1_1_event(void *ptr);
void next_btn_set_batch1_2_event(void *ptr);
void next_btn_set_batch1_3_event(void *ptr);
void next_btn_set_batch1_4_event(void *ptr); //belum
void next_btn_set_batch1_5_event(void *ptr);
void next_btn_set_batch1_6_event(void *ptr);
void next_btn_set_batch1_7_event(void *ptr);
void next_btn_set_batch1_8_event(void *ptr);
void next_btn_set_batch1_9_event(void *ptr);
void next_btn_set_batch2_0_event(void *ptr);
void next_btn_set_batch2_1_event(void *ptr);
void next_btn_set_batch2_2_event(void *ptr);
void next_btn_set_batch2_3_event(void *ptr);
void next_btn_set_batch2_4_event(void *ptr); 
void next_btn_set_batch2_5_event(void *ptr);
void next_btn_set_batch2_6_event(void *ptr);
void next_btn_set_batch2_7_event(void *ptr);
void next_btn_set_batch2_8_event(void *ptr);
void next_btn_set_batch2_9_event(void *ptr);
void nex_btn_ok_event(void *ptr);

void next_btn_ok_keyboard_event(void *ptr);
void next_btn_x_keyboard_event(void *ptr);
void nex_init_event_all(){
  nexInit();
  nex_set_liter[0].attachPush(nex_set_liter_0_event, &nex_set_liter[0]);
  nex_set_liter[1].attachPush(nex_set_liter_1_event, &nex_set_liter[1]);
  nex_factor_k[0].attachPush(nex_factor_k_0_event, &nex_factor_k[0]);
  nex_factor_k[1].attachPush(nex_factor_k_1_event, &nex_factor_k[1]);
  nex_f_kurang[0].attachPush(nex_f_kurang_0_event, &nex_f_kurang[0]);
  nex_f_kurang[1].attachPush(nex_f_kurang_1_event, &nex_f_kurang[1]);

  nex_btn_tank1.attachPush(nex_btn_tank1_event, &nex_btn_tank1);
  nex_btn_tank2.attachPush(nex_btn_tank2_event, &nex_btn_tank2);
  nex_btn_batch1.attachPush(nex_btn_batch1_event, &nex_btn_batch1);
  nex_btn_batch2.attachPush(nex_btn_batch2_event, &nex_btn_batch2);
  nex_btn_mode_produksi1.attachPush(nex_btn_mode_produksi1_event, &nex_btn_mode_produksi1);
  nex_btn_mode_produksi2.attachPush(nex_btn_mode_produksi2_event, &nex_btn_mode_produksi2);

  nex_capacity[0].attachPush(nex_capacity_0_event, &nex_capacity[0]);
  nex_capacity[1].attachPush(nex_capacity_1_event, &nex_capacity[1]);
  nex_over_fl_err[0].attachPush(nex_over_fl_err_0_event, &nex_over_fl_err[0]);
  nex_over_fl_err[1].attachPush(nex_over_fl_err_1_event, &nex_over_fl_err[1]);
  nex_delay_on[0].attachPush(nex_delay_on_0_event, &nex_delay_on[0]);
  nex_delay_on[1].attachPush(nex_delay_on_1_event, &nex_delay_on[1]);
  nex_delay_off[0].attachPush(nex_delay_off_0_event, &nex_delay_off[0]);
  nex_delay_off[1].attachPush(nex_delay_off_1_event, &nex_delay_off[1]);

  nex_btn_set_tank1[0].attachPush(nex_btn_set_tank1_0_event, &nex_btn_set_tank1[0]);
  nex_btn_set_tank1[1].attachPush(nex_btn_set_tank1_1_event, &nex_btn_set_tank1[1]);
  nex_btn_set_tank1[2].attachPush(nex_btn_set_tank1_2_event, &nex_btn_set_tank1[2]);
  nex_btn_set_tank1[3].attachPush(nex_btn_set_tank1_3_event, &nex_btn_set_tank1[3]);
  nex_btn_set_tank1[4].attachPush(nex_btn_set_tank1_4_event, &nex_btn_set_tank1[4]);

  nex_btn_set_tank2[0].attachPush(nex_btn_set_tank2_0_event, &nex_btn_set_tank2[0]);
  nex_btn_set_tank2[1].attachPush(nex_btn_set_tank2_1_event, &nex_btn_set_tank2[1]);
  nex_btn_set_tank2[2].attachPush(nex_btn_set_tank2_2_event, &nex_btn_set_tank2[2]);
  nex_btn_set_tank2[3].attachPush(nex_btn_set_tank2_3_event, &nex_btn_set_tank2[3]);
  nex_btn_main.attachPush(nex_btn_main_event, &nex_btn_main);

  next_btn_set_batch1[0].attachPush(next_btn_set_batch1_0_event, &next_btn_set_batch1[0]); 
  next_btn_set_batch1[1].attachPush(next_btn_set_batch1_1_event, &next_btn_set_batch1[1]); 
  next_btn_set_batch1[2].attachPush(next_btn_set_batch1_2_event, &next_btn_set_batch1[2]); 
  next_btn_set_batch1[3].attachPush(next_btn_set_batch1_3_event, &next_btn_set_batch1[3]); 
  next_btn_set_batch1[4].attachPush(next_btn_set_batch1_4_event, &next_btn_set_batch1[4]); 
  next_btn_set_batch1[5].attachPush(next_btn_set_batch1_5_event, &next_btn_set_batch1[5]); 
  next_btn_set_batch1[6].attachPush(next_btn_set_batch1_6_event, &next_btn_set_batch1[6]); 
  next_btn_set_batch1[7].attachPush(next_btn_set_batch1_7_event, &next_btn_set_batch1[7]); 
  next_btn_set_batch1[8].attachPush(next_btn_set_batch1_8_event, &next_btn_set_batch1[8]); 
  next_btn_set_batch1[9].attachPush(next_btn_set_batch1_9_event, &next_btn_set_batch1[9]);
  next_btn_set_batch2[0].attachPush(next_btn_set_batch2_0_event, &next_btn_set_batch2[0]);
  next_btn_set_batch2[1].attachPush(next_btn_set_batch2_1_event, &next_btn_set_batch2[1]); 
  next_btn_set_batch2[2].attachPush(next_btn_set_batch2_2_event, &next_btn_set_batch2[2]); 
  next_btn_set_batch2[3].attachPush(next_btn_set_batch2_3_event, &next_btn_set_batch2[3]); 
  next_btn_set_batch2[4].attachPush(next_btn_set_batch2_4_event, &next_btn_set_batch2[4]); 
  next_btn_set_batch2[5].attachPush(next_btn_set_batch2_5_event, &next_btn_set_batch2[5]); 
  next_btn_set_batch2[6].attachPush(next_btn_set_batch2_6_event, &next_btn_set_batch2[6]); 
  next_btn_set_batch2[7].attachPush(next_btn_set_batch2_7_event, &next_btn_set_batch2[7]); 
  next_btn_set_batch2[8].attachPush(next_btn_set_batch2_8_event, &next_btn_set_batch2[8]); 
  next_btn_set_batch2[9].attachPush(next_btn_set_batch2_9_event, &next_btn_set_batch2[9]);
  nex_btn_ok.attachPush(nex_btn_ok_event, &nex_btn_ok);

  next_btn_ok_keyboard.attachPop(next_btn_ok_keyboard_event, &next_btn_ok_keyboard); 
  next_btn_x_keyboard.attachPop(next_btn_x_keyboard_event, &next_btn_x_keyboard);
}
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
  nex_init_event_all();
  index_page_change = !index_page;
  setTime(mb_int_date_time.values.hour, mb_int_date_time.values.minute, mb_int_date_time.values.second, mb_int_date_time.values.day, mb_int_date_time.values.month, mb_int_date_time.values.year); // --> setTime(hr,min,sec,day,mnth,yr);
  nex_show_date_time();
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
      if(addr_mb_coil < 5) { set_mb_flag(addr_mb_coil, 0, 5); index_page_change = !index_page; flag_next_set_val = false; }
      else if(addr_mb_coil >= 5 && addr_mb_coil < 9) {set_mb_flag(addr_mb_coil, 5, 9); index_page_change = !index_page; flag_next_set_val = false; }
      else if(addr_mb_coil >= 9 && addr_mb_coil < 19) {set_mb_flag(addr_mb_coil, 9, 19); index_page_change = !index_page; flag_next_set_val = false; }
      else if(addr_mb_coil >= 19 && addr_mb_coil < 29) {set_mb_flag(addr_mb_coil, 19, 29); index_page_change = !index_page; flag_next_set_val = false; }
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
        if(index < 5){ set_mb_flag(index, 0, 5); index_page_change = !index_page; flag_next_set_val = false; }
        else if(index >= 5 && index < 9) { set_mb_flag(index, 5, 9); index_page_change = !index_page; flag_next_set_val = false;}
        else if(index >= 9 && index < 19) { set_mb_flag(index, 9, 19); index_page_change = !index_page; flag_next_set_val = false;}
        else if(index >= 19 && index < 29) { set_mb_flag(index, 19, 29); index_page_change = !index_page; flag_next_set_val = false;}
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
void nex_show_date_time(){
  char buffer_date_time[20];
  if(mb_int_date_time.values.day != day()) mb_int_date_time.values.day = day();
  if(mb_int_date_time.values.month != month()) mb_int_date_time.values.month = month();
  if(mb_int_date_time.values.year != year()) mb_int_date_time.values.year = year();
  if(mb_int_date_time.values.hour != hour()) mb_int_date_time.values.hour = hour();
  if(mb_int_date_time.values.minute != minute()) mb_int_date_time.values.minute = minute();
  if(mb_int_date_time.values.second != second()) mb_int_date_time.values.second = second();
  sprintf(buffer_date_time, "%02u-%02u-%04u %02u:%02u:%02u", mb_int_date_time.values.day, mb_int_date_time.values.month, mb_int_date_time.values.year, mb_int_date_time.values.hour, mb_int_date_time.values.minute, mb_int_date_time.values.second);
  nex_date_time.setText(buffer_date_time);
}
void nex_show_value(){
  if(index_page == 0){
    if(IsConnectTCP_Prev != IsConnectTCP){ IsConnectTCP_Prev = IsConnectTCP; nex_com_conn.setText(IsConnectTCP ? "COM Connected" : "COM Disconnect"); nex_com_conn.Set_font_color_pco(IsConnectTCP ? 1024 : 63488); }
    if(flag_prev_log[0] != flagCoil[0][1]){ flag_prev_log[0] = flagCoil[0][1]; nex_log[0].setValue(flagCoil[0][1]); }
    if(flag_prev_log[1] != flagCoil[1][1]){ flag_prev_log[1] = flagCoil[1][1]; nex_log[1].setValue(flagCoil[1][1]); }
    if(mode_prev[0] != digitalRead(X[4])){
      mode_prev[0] = digitalRead(X[4]);
      flag_next_setvalue = true;
      nex_mode[0].setText(mode_prev[0] ? "Manual" : "Auto" );
      flag_next_setvalue = false;
    }
    if(mode_prev[1] != digitalRead(X[5])){
      mode_prev[1] = digitalRead(X[5]);
      flag_next_setvalue = true;
      nex_mode[1].setText(mode_prev[1] ? "Manual" : "Auto" );
      flag_next_setvalue = false;
    }
    if(mb_int_date_time.values.second != second()) nex_show_date_time();
  }
  nex_read_value();
  if(millis() - time_show_value >= 1000){
    time_show_value = millis();
    if(index_page == 0){
      flag_next_setvalue = true;
      nex_liter[0].setValue(mbFloat[0].values.liter * 100); nex_liter[1].setValue(mbFloat[1].values.liter * 100);
      nex_FlowRate[0].setValue(mbFloat[0].values.Flowrate * 100); nex_FlowRate[1].setValue(mbFloat[1].values.Flowrate * 100);
      flag_next_setvalue = false;
    }
    if(index_page_change != index_page && !flag_next_set_val){
      if(index_page == 0){
        flag_next_setvalue = true;
        nex_set_liter[0].setValue(mbFloat[0].values.setPoint * 100); nex_set_liter[1].setValue(mbFloat[1].values.setPoint * 100);
        nex_factor_k[0].setValue(mbFloat[0].values.kFact * 10000); nex_factor_k[1].setValue(mbFloat[1].values.kFact * 10000);
        nex_f_kurang[0].setValue(mbFloat[0].values.factorKurang * 100); nex_f_kurang[1].setValue(mbFloat[1].values.factorKurang * 100);
        for (int i = 0; i < 5; i++){
          if(mb_flagCoil[i]) nex_btn_tank1.setText(tanki_1_char[i]);
        }
        for (int i = 5; i < 9; i++){
          if(mb_flagCoil[i]) nex_btn_tank2.setText(tanki_2_char[i - 5]);
        }
        for (int i = 9; i < 19; i++){
          if(mb_flagCoil[i]) nex_btn_batch1.setText(batch_char[i-9]);
        }
        for (int i = 19; i < 29; i++){
          if(mb_flagCoil[i]) nex_btn_batch2.setText(batch_char[i-19]);
        }
        nex_btn_mode_produksi1.setText(mode_produksi_persiapan[0] ? "Persiapan" : "Produksi");
        nex_btn_mode_produksi1.Set_background_color_bco(mode_produksi_persiapan[0] ? 63488 : 1040);
        nex_btn_mode_produksi1.Set_press_background_color_bco2(mode_produksi_persiapan[0] ? 63488 : 1040);
        nex_btn_mode_produksi2.setText(mode_produksi_persiapan[1] ? "Persiapan" : "Produksi");
        nex_btn_mode_produksi2.Set_background_color_bco(mode_produksi_persiapan[1] ? 63488 : 1040);
        nex_btn_mode_produksi2.Set_press_background_color_bco2(mode_produksi_persiapan[1] ? 63488 : 1040);
        flag_next_setvalue = false;
        IsConnectTCP_Prev = !IsConnectTCP;
      }
      if(index_page == 1){
        flag_next_setvalue = true;
        nex_capacity[0].setValue(mbFloat[0].values.capacity * 100);
        nex_capacity[1].setValue(mbFloat[1].values.capacity * 100);
        nex_over_fl_err[0].setValue(mbFloat[0].values.over_fl_err);
        nex_over_fl_err[1].setValue(mbFloat[1].values.over_fl_err);
        nex_delay_on[0].setValue(mbFloat[0].values.timeInterval_OnValve);
        nex_delay_on[1].setValue(mbFloat[1].values.timeInterval_OnValve);
        nex_delay_off[0].setValue(mbFloat[0].values.timeInterval_OffValve);
        nex_delay_off[1].setValue(mbFloat[1].values.timeInterval_OffValve);
        flag_next_setvalue = false;
        nex_show_set_and_tank();
      }
      if(index_page == 3){
        nex_show_batch();
      }
      index_page_change = index_page;
    }
  }
}
void nex_show_set_and_tank() {
    if (index_page == 1) {
        flag_next_setvalue = true;
        for (int i = 0; i < 5; ++i) {
            nex_btn_set_tank1[i].Set_background_color_bco(mb_flagCoil[i] ? 1024 : 63488);
            if(i < 4) nex_btn_set_tank2[i].Set_background_color_bco(mb_flagCoil[5 + i] ? 1024 : 63488);
        }
        flag_next_setvalue = false;
    }
}
void nex_show_batch() {
    if (index_page == 3) {
        flag_next_setvalue = true;
        for (int i = 0; i < 10; ++i) {
            next_btn_set_batch1[i].Set_background_color_bco(mb_flagCoil[9 + i] ? 1024 : 63488);
            next_btn_set_batch2[i].Set_background_color_bco(mb_flagCoil[19 + i] ? 1024 : 63488);
        }
        flag_next_setvalue = false;
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
        prefereces_partition1(false);
        break;
      case 2:
        nex_set_liter[1].getValue(&value);
        mbFloat[1].values.setPoint = (float)value/100;
        prefereces_partition2(false);
        break;
      case 3:
        nex_factor_k[0].getValue(&value);
        mbFloat[0].values.kFact = (float)value/10000;
        prefereces_partition1(false);
        break;
      case 4:
        nex_factor_k[1].getValue(&value);
        mbFloat[1].values.kFact = (float)value/10000;
        prefereces_partition2(false);
        break;
      case 5:
        nex_f_kurang[0].getValue(&value);
        mbFloat[0].values.factorKurang = (float)value/100;
        prefereces_partition1(false);
        break;
      case 6:
        nex_f_kurang[1].getValue(&value);
        mbFloat[1].values.factorKurang = (float)value/100;
        prefereces_partition2(false);
        break;
      case 7:
        nex_capacity[0].getValue(&value);
        mbFloat[0].values.capacity = (float)value/100;
        prefereces_partition1(false);
        break;
      case 8:
        nex_capacity[1].getValue(&value);
        mbFloat[1].values.capacity = (float)value/100;
        prefereces_partition2(false);
        break;
      case 9:
        nex_over_fl_err[0].getValue(&value);
        mbFloat[0].values.over_fl_err = (float)value;
        prefereces_partition1(false);
        break;
      case 10:
        nex_over_fl_err[1].getValue(&value);
        mbFloat[1].values.over_fl_err = (float)value;
        prefereces_partition2(false);
      case 11:
        nex_delay_on[0].getValue(&value);
        mbFloat[0].values.timeInterval_OnValve = (float)value;
        prefereces_partition1(false);
        break;
      case 12:
        nex_delay_on[1].getValue(&value);
        mbFloat[1].values.timeInterval_OnValve = (float)value;
        prefereces_partition2(false);
        break;
      case 13:
        nex_delay_off[0].getValue(&value);
        mbFloat[0].values.timeInterval_OffValve = (float)value;
        prefereces_partition1(false);
        break;
      case 14:
        nex_delay_off[1].getValue(&value);
        mbFloat[1].values.timeInterval_OffValve = (float)value;
        prefereces_partition2(false);
        break;
    }
    flag_next_set_val = false;
    time_show_value = millis();
  }
}
void nex_set_value_for_event(void *ptr, int index_setval_){
  if(flag_next_read_val || flag_next_setvalue) { 
    page[index_page].show();
    index_page_change = 2; 
    time_show_value = millis(); 
    return; 
  }
  flag_next_set_val = true;
  index_page_prev = index_page;
  index_page_change = index_page = 2;
  index_setval = index_setval_;
  time_show_value = millis();
  flag_next_set_val = false;
}
void nex_set_liter_0_event(void *ptr){ nex_set_value_for_event(ptr, 1); }
void nex_set_liter_1_event(void *ptr){ nex_set_value_for_event(ptr, 2); }
void nex_factor_k_0_event(void *ptr){ nex_set_value_for_event(ptr, 3); }
void nex_factor_k_1_event(void *ptr){ nex_set_value_for_event(ptr, 4); }
void nex_f_kurang_0_event(void *ptr){ nex_set_value_for_event(ptr, 5); }
void nex_f_kurang_1_event(void *ptr){ nex_set_value_for_event(ptr, 6); }
void nex_capacity_0_event(void *ptr){ nex_set_value_for_event(ptr, 7); }
void nex_capacity_1_event(void *ptr){ nex_set_value_for_event(ptr, 8); }
void nex_over_fl_err_0_event(void *ptr){ nex_set_value_for_event(ptr, 9); }
void nex_over_fl_err_1_event(void *ptr){ nex_set_value_for_event(ptr, 10); }
void nex_delay_on_0_event(void *ptr){ nex_set_value_for_event(ptr, 11); }
void nex_delay_on_1_event(void *ptr){ nex_set_value_for_event(ptr, 12); }
void nex_delay_off_0_event(void *ptr){ nex_set_value_for_event(ptr, 13); }
void nex_delay_off_1_event(void *ptr){ nex_set_value_for_event(ptr, 14); }
void nex_btn_set_tank_event(void *ptr, int index, int start_index, int end_index){
  mb_flagCoil[index] = true; 
  set_mb_flag(index, start_index, end_index);
  nex_show_set_and_tank();
}
void nex_btn_set_batch_event(void *ptr, int index, int start_index, int end_index){
  mb_flagCoil[index] = true; 
  set_mb_flag(index, start_index, end_index);
  nex_show_batch();
}
void nex_btn_set_tank1_0_event(void *ptr){ nex_btn_set_tank_event(ptr, 0, 0, 5); }
void nex_btn_set_tank1_1_event(void *ptr){ nex_btn_set_tank_event(ptr, 1, 0, 5); }
void nex_btn_set_tank1_2_event(void *ptr){ nex_btn_set_tank_event(ptr, 2, 0, 5); }
void nex_btn_set_tank1_3_event(void *ptr){ nex_btn_set_tank_event(ptr, 3, 0, 5); }
void nex_btn_set_tank1_4_event(void *ptr){ nex_btn_set_tank_event(ptr, 4, 0, 5); }

void nex_btn_set_tank2_0_event(void *ptr){ nex_btn_set_tank_event(ptr, 5, 5, 9); }
void nex_btn_set_tank2_1_event(void *ptr){ nex_btn_set_tank_event(ptr, 6, 5, 9); }
void nex_btn_set_tank2_2_event(void *ptr){ nex_btn_set_tank_event(ptr, 7, 5, 9); }
void nex_btn_set_tank2_3_event(void *ptr){ nex_btn_set_tank_event(ptr, 8, 5, 9); }

void next_btn_set_batch1_0_event(void *ptr){ nex_btn_set_batch_event(ptr, 9, 9, 19); }
void next_btn_set_batch1_1_event(void *ptr){ nex_btn_set_batch_event(ptr, 10, 9, 19); }
void next_btn_set_batch1_2_event(void *ptr){ nex_btn_set_batch_event(ptr, 11, 9, 19); }
void next_btn_set_batch1_3_event(void *ptr){ nex_btn_set_batch_event(ptr, 12, 9, 19); }
void next_btn_set_batch1_4_event(void *ptr){ nex_btn_set_batch_event(ptr, 13, 9, 19); }
void next_btn_set_batch1_5_event(void *ptr){ nex_btn_set_batch_event(ptr, 14, 9, 19); }
void next_btn_set_batch1_6_event(void *ptr){ nex_btn_set_batch_event(ptr, 15, 9, 19); }
void next_btn_set_batch1_7_event(void *ptr){ nex_btn_set_batch_event(ptr, 16, 9, 19); }
void next_btn_set_batch1_8_event(void *ptr){ nex_btn_set_batch_event(ptr, 17, 9, 19); }
void next_btn_set_batch1_9_event(void *ptr){ nex_btn_set_batch_event(ptr, 18, 9, 19); }

void next_btn_set_batch2_0_event(void *ptr){ nex_btn_set_batch_event(ptr, 19, 19, 29); }
void next_btn_set_batch2_1_event(void *ptr){ nex_btn_set_batch_event(ptr, 20, 19, 29); }
void next_btn_set_batch2_2_event(void *ptr){ nex_btn_set_batch_event(ptr, 21, 19, 29); }
void next_btn_set_batch2_3_event(void *ptr){ nex_btn_set_batch_event(ptr, 22, 19, 29); }
void next_btn_set_batch2_4_event(void *ptr){ nex_btn_set_batch_event(ptr, 23, 19, 29); }
void next_btn_set_batch2_5_event(void *ptr){ nex_btn_set_batch_event(ptr, 24, 19, 29); }
void next_btn_set_batch2_6_event(void *ptr){ nex_btn_set_batch_event(ptr, 25, 19, 29); }
void next_btn_set_batch2_7_event(void *ptr){ nex_btn_set_batch_event(ptr, 26, 19, 29); }
void next_btn_set_batch2_8_event(void *ptr){ nex_btn_set_batch_event(ptr, 27, 19, 29); }
void next_btn_set_batch2_9_event(void *ptr){ nex_btn_set_batch_event(ptr, 28, 19, 29); }

void nex_btn_set_all_event(void *ptr, int page_index){
  if(flag_next_setvalue) return;
  if(index_page_change != index_page) return;
  flag_next_set_val = true;
  
  page[page_index].show();
  time_show_value = millis();
  flag_next_set_val = false;
  index_page = page_index;
}
void nex_btn_main_event(void *ptr){ nex_btn_set_all_event(ptr, 0); }
void nex_btn_ok_event(void *ptr){ nex_btn_set_all_event(ptr, 0); }
void nex_btn_tank1_event(void *ptr){ nex_btn_set_all_event(ptr, 1); }
void nex_btn_tank2_event(void *ptr){ nex_btn_set_all_event(ptr, 1); }
void nex_btn_batch1_event(void *ptr){ nex_btn_set_all_event(ptr, 3); }
void nex_btn_batch2_event(void *ptr){ nex_btn_set_all_event(ptr, 3); }
void nex_btn_mode_produksi1_event(void *ptr){
  mode_produksi_persiapan[0] = !mode_produksi_persiapan[0];
  nex_btn_mode_produksi1.setText(mode_produksi_persiapan[0] ? "Persiapan" : "Produksi");
  nex_btn_mode_produksi1.Set_background_color_bco(mode_produksi_persiapan[0] ? 63488 : 1040);
  nex_btn_mode_produksi1.Set_press_background_color_bco2(mode_produksi_persiapan[0] ? 63488 : 1040);
}
void nex_btn_mode_produksi2_event(void *ptr){
  mode_produksi_persiapan[1] = !mode_produksi_persiapan[1];
  nex_btn_mode_produksi2.setText(mode_produksi_persiapan[1] ? "Persiapan" : "Produksi");
  nex_btn_mode_produksi2.Set_background_color_bco(mode_produksi_persiapan[1] ? 63488 : 1040);
  nex_btn_mode_produksi2.Set_press_background_color_bco2(mode_produksi_persiapan[1] ? 63488 : 1040);
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