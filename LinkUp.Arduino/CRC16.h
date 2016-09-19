// CRC16.h

#ifndef _CRC16_h
#define _CRC16_h

#include <Arduino.h>

class CRC16Class
{
public:
	uint16_t calc(uint8_t *pData, uint32_t nCount);
};

extern CRC16Class CRC16;

#endif