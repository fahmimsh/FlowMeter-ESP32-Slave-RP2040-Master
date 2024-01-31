#include <Arduino.h>

const int pwmFrequency = 2000;
const int pwmResolution = 8;

void setup() {
  ledcSetup(0, pwmFrequency, pwmResolution);
  ledcSetup(1, pwmFrequency, pwmResolution);
  ledcSetup(2, pwmFrequency, pwmResolution);
  ledcSetup(3, pwmFrequency, pwmResolution);
  ledcAttachPin(16, 0);
  ledcAttachPin(4, 1);
  ledcAttachPin(2, 2);
  ledcAttachPin(13, 3);
}

void loop() {
  ledcWrite(0, 128);
  ledcWrite(1, 128);
  ledcWrite(2, 128);
  ledcWrite(3, 128);
}
