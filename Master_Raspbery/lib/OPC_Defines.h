#include <Arduino.h>
const char *ItemD[10] = {"d0", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "d9"}; 
const char *ItemC[28] = {
    "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9", "c10", "c11", "c12", "c13", "c14",
    "c15", "c16", "c17", "c18", "c19", "c20", "c21", "c22", "c23", "c24", "c25", "c26", "c27"
};
const char *ItemI[6] = { "i0", "i1", "i2", "i3", "i4", "i5" };
const char *ItemF[12] = { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9", "f10", "f11" };
const uint8_t size_item_D = sizeof(ItemD)/sizeof(ItemD[0]);
const uint8_t size_item_C = sizeof(ItemC)/sizeof(ItemC[0]);
const uint8_t size_item_I = sizeof(ItemI)/sizeof(ItemI[0]);
const uint8_t size_item_F = sizeof(ItemF)/sizeof(ItemF[0]);