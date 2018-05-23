/**
* Author: Thomas Weichselbaumer
* Version: 1.0.0
* File Name: CRC16.h
* Description: Header file for the LinkUp lib.
**/

#ifndef _CRC16_h
#define _CRC16_h

#include "Platform.h"
#ifdef LINKUP_BOOST_CRC16
#include <boost/crc.hpp>
#endif

class CRC16
{
public:
	static uint16_t calc(uint8_t *pData, uint32_t nCount);
};

#endif