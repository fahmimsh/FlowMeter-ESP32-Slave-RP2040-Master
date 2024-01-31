#include <Arduino.h>
const char *ItemD[12] = {"d0", "d1", "d2", "d3", "d4", "d5", "d6", "d7", "d8", "d9", "d10", "d11"}; 
const char *ItemC[40] = {
    "c0", "c1", "c2", "c3", "c4", "c5", "c6", "c7", "c8", "c9",
    "c10", "c11", "c12", "c13", "c14", "c15", "c16", "c17", "c18", "c19",
    "c20", "c21", "c22", "c23", "c24", "c25", "c26", "c27", "c28", "c29",
    "c30", "c31", "c32", "c33", "c34", "c35", "c36", "c37", "c38", "c39",
};
const char *ItemI[15] = { "i0", "i1", "i2", "i3", "i4", "i5", "i6", "i7", "i8", "i9", "i10", "i11", "i12", "i13", "i14" };
const char *ItemF[20] = {
    "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9",
    "f10", "f11", "f12", "f13", "f14", "f15", "f16", "f17", "f18", "f19",
};
const uint8_t size_item_D = sizeof(ItemD)/sizeof(ItemD[0]);
const uint8_t size_item_C = sizeof(ItemC)/sizeof(ItemC[0]);
const uint8_t size_item_I = sizeof(ItemI)/sizeof(ItemI[0]);
const uint8_t size_item_F = sizeof(ItemF)/sizeof(ItemF[0]);