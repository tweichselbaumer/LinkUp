#ifndef SYSTEMTIME_h
#define SYSTEMTIME_h

#include "Platform.h"

 class PACKED BigTimeStamp {
public:
	uint32_t lowerTime;
	uint32_t upperTime;
	BigTimeStamp operator-(const BigTimeStamp &b);
};


class SystemTime {
public:
	SystemTime();
	void update();
	BigTimeStamp time;
};

#endif