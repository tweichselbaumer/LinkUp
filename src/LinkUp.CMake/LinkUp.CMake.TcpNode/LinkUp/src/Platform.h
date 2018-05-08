#ifndef _PLATFORM_h
#define _PLATFORM_h

#ifdef ARDUINO
#include <Arduino.h>
#define PACKED  __attribute__((packed))
#endif

#ifdef _WINDOWS
#include <stdint.h>
#include <memory.h>
#include <string.h>
#include <stdlib.h>
#include <malloc.h>
#define PACKED
#pragma pack(1)
#include <time.h>
//#define micros (uint32_t)1000*1000/CLOCKS_PER_SEC*clock
#endif

#ifdef __linux
#include <stdint.h>
#include <string.h>
#include <stdlib.h>
#define PACKED
#pragma pack(1)
#include <time.h>
#define micros (uint32_t)1000*1000/CLOCKS_PER_SEC*clock
#endif

#endif