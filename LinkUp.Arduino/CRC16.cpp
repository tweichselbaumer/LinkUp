#include "CRC16.h"

uint16_t CRC16Class::calc(uint8_t *pData, uint32_t nCount)
{
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
}

CRC16Class CRC16;