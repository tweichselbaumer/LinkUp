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
	uint32_t nLength;
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
	ReceiveLength3 = 4,
	ReceiveLength4 = 5,
	ReceiveData = 6,
	ReceiveCRC = 7,
	ReceiveCheckCRC = 8,
	ReceiveEnd = 9,
	SendPreamble = 10,
	SendLength1 = 11,
	SendLength2 = 12,
	SendLength3 = 13,
	SendLength4 = 14,
	SendData = 15,
	SendCrc1 = 16,
	SendCrc2 = 17,
	SendEnd = 18,
	SendIdle = 19
};

class LinkUpRaw
{
private:
	LinkUpState stateIn = LinkUpState::ReceivePreamble;
	LinkUpState stateOut = LinkUpState::SendIdle;
	bool skipIn = false;
	bool skipOut = false;
	uint32_t nBytesToRead;
	uint32_t nBytesToSend;
	LinkUpPacketList* pHeadIn = 0;
	LinkUpPacketList* pTailIn = 0;
	LinkUpPacketList* pHeadOut = 0;
	LinkUpPacketList* pTailOut = 0;
	LinkUpPacketList* pProgressingIn = 0;
	LinkUpPacketList* pProgressingOut = 0;
	bool checkForError(uint8_t nByte);
#ifdef LINKUP_BOOST_THREADSAFE
	boost::mutex mtx_queue;
	boost::mutex mtx_progressing;
#endif
	void lock_queue();
	void unlock_queue();
	void lock_progressing();
	void unlock_progressing();
public:
	void progress(uint8_t* pData, uint32_t nCount);
	void send(LinkUpPacket packet);
	void send(LinkUpPacket packet, bool bWithCrc);
	uint16_t getRaw(uint8_t* pData, uint32_t nMax);
	bool hasNext();
	LinkUpPacket next();
	uint32_t nTotalFailedPackets;
	uint32_t nTotalReceivedPackets;
	uint32_t nTotalSendPackets;
	uint64_t nTotalSendBytes;
	uint64_t nTotalReceivedBytes;
};

#endif
