// LinkUp.h
//-std=c++0x

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

struct LinkUpHeader
{
	uint8_t lenght;
};

struct LinkUpPacket
{
	LinkUpHeader header;
	uint8_t *data;
	uint16_t crc;
};

struct LinkUpPacketList
{
	LinkUpPacket packet;
	LinkUpPacketList *next;
};

enum LinkUpState
{
	ReceivePreamble = 1,
	ReceiveLength = 2,
	ReceiveData = 3,
	ReceiveCRC = 4,
	ReceiveCheckCRC = 5,
	ReceiveEnd = 6,
	SendPreamble = 7,
	SendLenght = 8,
	SendData = 9,
	SendSkip = 10,
	SendCrc1 = 11,
	SendCrc2 = 12,
	SendEnd = 13,
	SendIdle = 14
};

class LinkUpClass
{
private:
	LinkUpState stateIn = LinkUpState::ReceivePreamble;
	LinkUpState stateOut = LinkUpState::SendIdle;
	bool skipIn = false;
	bool skipOut = false;
	uint8_t nBytesToRead;
	uint8_t nBytesToSend;
	LinkUpPacketList* pHeadIn;
	LinkUpPacketList* pTailIn;
	LinkUpPacketList* pHeadOut;
	LinkUpPacketList* pTailOut;
	LinkUpPacketList* pProgressingIn;
	LinkUpPacketList* pProgressingOut;
	bool checkForError(uint8_t nByte);
public:
	void progress(uint8_t* pData, uint8_t nCount);
	void send(LinkUpPacket packet);
	uint8_t getRaw(uint8_t* pData, uint8_t nMax);
	bool hasNext();
	LinkUpPacket next();
	uint32_t nTotalFailedPackets;
	uint32_t nTotalReceivedPackets;
};

extern LinkUpClass LinkUp;

#endif
