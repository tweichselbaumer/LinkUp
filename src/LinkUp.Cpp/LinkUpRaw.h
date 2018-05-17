/**
 * Author: Thomas Weichselbaumer
 * Version: 1.0.0
 * File Name: LinkUp.h
 * Description: Header file for the LinkUp lib.
 **/

#ifndef _LINKUP_RAW_h
#define _LINKUP_RAW_h

#include "Platform.h"

#include "CRC16.h"

#ifndef LINKUP_RAW_PREAMBLE
#define LINKUP_RAW_PREAMBLE 0xAA
#endif

#ifndef LINKUP_RAW_EOP
#define LINKUP_RAW_EOP 0x99
#endif

#ifndef LINKUP_RAW_SKIP
#define LINKUP_RAW_SKIP 0x55
#endif

#ifndef LINKUP_RAW_XOR
#define LINKUP_RAW_XOR 0x20
#endif

struct LinkUpPacket
{
	uint16_t nLength;
	uint8_t *pData;
	uint16_t nCrc;
};

struct LinkUpPacketList
{
	LinkUpPacket packet;
	LinkUpPacketList *next;
};

enum LinkUpState :uint8_t
{
	ReceivePreamble = 1,
	ReceiveLength1 = 2,
	ReceiveLength2 = 3,
	ReceiveData = 4,
	ReceiveCRC = 5,
	ReceiveCheckCRC = 6,
	ReceiveEnd = 7,
	SendPreamble = 8,
	SendLength1 = 9,
	SendLength2 = 10,
	SendData = 11,
	SendCrc1 = 12,
	SendCrc2 = 13,
	SendEnd = 14,
	SendIdle = 15
};

class LinkUpRaw
{
private:
	LinkUpState stateIn = LinkUpState::ReceivePreamble;
	LinkUpState stateOut = LinkUpState::SendIdle;
	bool skipIn = false;
	bool skipOut = false;
	uint16_t nBytesToRead;
	uint16_t nBytesToSend;
	LinkUpPacketList* pHeadIn = 0;
	LinkUpPacketList* pTailIn = 0;
	LinkUpPacketList* pHeadOut = 0;
	LinkUpPacketList* pTailOut = 0;
	LinkUpPacketList* pProgressingIn = 0;
	LinkUpPacketList* pProgressingOut = 0;
	bool checkForError(uint16_t nByte);
public:
	void progress(uint8_t* pData, uint16_t nCount);
	void send(LinkUpPacket packet);
	uint16_t getRaw(uint8_t* pData, uint16_t nMax);
	bool hasNext();
	LinkUpPacket next();
	uint32_t nTotalFailedPackets;
	uint32_t nTotalReceivedPackets;
	uint32_t nTotalSendPackets;
	uint64_t nTotalSendBytes;
	uint64_t nTotalReceivedBytes;
};

#endif
