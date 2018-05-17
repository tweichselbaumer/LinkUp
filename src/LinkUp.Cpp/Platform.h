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
#include <cstdlib>
#include <iostream>
#include <fstream>
#endif

#ifdef __linux
#include <stdint.h>
#include <string.h>
#include <stdlib.h>
#define PACKED
#pragma pack(1)
#include <time.h>
#include <cstdlib>
#include <iostream>
#include <fstream>
#endif


#define min(a,b) (((a)<(b))?(a):(b))
#define max(a,b) (((a)>(b))?(a):(b))

#ifdef LINKUP_BOOST_THREADSAFE
#include <boost/thread.hpp>
#endif

#endif