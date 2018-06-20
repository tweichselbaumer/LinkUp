#ifndef _PLATFORM_h
#define _PLATFORM_h

#ifdef ARDUINO
#include <Arduino.h>
#define PACK( __Declaration__ ) __Declaration__ __attribute__((__packed__))
#endif

#ifdef _WINDOWS
#include <stdint.h>
#include <memory.h>
#include <string.h>
#include <stdlib.h>
#include <malloc.h>
#define PACK( __Declaration__ ) __pragma( pack(push, 1) ) __Declaration__ __pragma( pack(pop) )
#include <time.h>
#include <cstdlib>
#include <iostream>
#include <fstream>
#endif

#ifdef __linux
#include <stdint.h>
#include <string.h>
#include <stdlib.h>
#define PACK( __Declaration__ ) __Declaration__ __attribute__((__packed__))
#include <time.h>
#include <cstdlib>
#include <iostream>
#include <fstream>
#endif

#define linkup_min(a,b) (((a)<(b))?(a):(b))
#define linkup_max(a,b) (((a)>(b))?(a):(b))

#ifdef LINKUP_BOOST_THREADSAFE
#include <boost/thread.hpp>
#endif

uint32_t getSystemTime();

#endif