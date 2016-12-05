/**
 * Author: Thomas Weichselbaumer
 * Version: 1.0.0
 * File Name: LinkUp.h
 * Description: Header file for the LinkUp lib.
 **/

#ifndef _LINKUP_h
#define _LINKUP_h

#include <Arduino.h>

#include "CRC16.h"

#ifndef LINKUP_PREAMBLE
#define LINKUP_PREAMBLE 0xAA
#endif

#ifndef LINKUP_EOP
#define LINKUP_EOP 0x99
#endif

#ifndef LINKUP_SKIP
#define LINKUP_SKIP 0x55
#endif

#ifndef LINKUP_XOR
#define LINKUP_XOR 0x20
#endif

struct LinkUpPacket
{
	uint16_t nLenght;
	uint8_t *pData;
	uint16_t nCrc;
};

struct LinkUpPacketList
{
	LinkUpPacket packet;
	LinkUpPacketList *next;
};

enum LinkUpState
{
	ReceivePreamble = 1,
	ReceiveLength1 = 2,
	ReceiveLength2 = 3,
	ReceiveData = 4,
	ReceiveCRC = 5,
	ReceiveCheckCRC = 6,
	ReceiveEnd = 7,
	SendPreamble = 8,
	SendLenght1 = 9,
	SendLenght2 = 10,
	SendData = 11,
	SendCrc1 = 12,
	SendCrc2 = 13,
	SendEnd = 14,
	SendIdle = 15
};

class LinkUpClass
{
private:
	LinkUpState stateIn = LinkUpState::ReceivePreamble;
	LinkUpState stateOut = LinkUpState::SendIdle;
	bool skipIn = false;
	bool skipOut = false;
	uint16_t nBytesToRead;
	uint16_t nBytesToSend;
	LinkUpPacketList* pHeadIn;
	LinkUpPacketList* pTailIn;
	LinkUpPacketList* pHeadOut;
	LinkUpPacketList* pTailOut;
	LinkUpPacketList* pProgressingIn;
	LinkUpPacketList* pProgressingOut;
	bool checkForError(uint16_t nByte);
public:
	void progress(uint8_t* pData, uint16_t nCount);
	void send(LinkUpPacket packet);
	uint16_t getRaw(uint8_t* pData, uint16_t nMax);
	bool hasNext();
	LinkUpPacket next();
	uint32_t nTotalFailedPackets;
	uint32_t nTotalReceivedPackets;
};

extern LinkUpClass LinkUp;

#endif
