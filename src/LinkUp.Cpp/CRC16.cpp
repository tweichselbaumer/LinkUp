/**
* Author: Thomas Weichselbaumer
* Version: 1.0.0
* File Name: CRC16.cpp
* Description: Source file for the LinkUp lib.
**/

#include "CRC16.h"

uint16_t CRC16::calc(uint8_t *pData, uint32_t nCount)
{
#ifdef LINKUP_BOOST_CRC16
	boost::crc_optimal<16, 0x1021, 0x0000, 0, false, false>  crc_ccitt2;
	crc_ccitt2 = std::for_each(pData, pData + nCount, crc_ccitt2);
	return crc_ccitt2();
#else
	uint16_t  crc;
	uint8_t i;

	crc = 0;
	nCount++;
	while (--nCount > 0)
	{
		crc = crc ^ (uint8_t)*pData++ << 8;
		i = 8;
		do
		{
			if (crc & 0x8000)
				crc = crc << 1 ^ 0x1021;
			else
				crc = crc << 1;
		} while (--i);
	}
	return (crc);
#endif // BOOST_CRC16
}