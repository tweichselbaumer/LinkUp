#include "LinkUp.h"

LinkUpPacket LinkUpClass::next() {
	LinkUpPacketList *pPacketList;
	LinkUpPacket packet;
	pPacketList = pHeadIn;
	if (pPacketList != NULL) {
		pHeadIn = pPacketList->next;
		packet = pPacketList->packet;
		free(pPacketList);
	}
	return packet;
}

bool LinkUpClass::hasNext() {
	return pHeadIn != NULL;
}

bool LinkUpClass::checkForError(uint8_t nByte) {
	if (nByte == LINKUP_EOP || nByte == LINKUP_PREAMBLE)
	{
		nTotalFailedPackets++;

		if (pProgressingIn->packet.data)
		{
			free(pProgressingIn->packet.data);
		}

		if (nByte == LINKUP_PREAMBLE)
		{
			stateIn = ReceiveLength;
		}

		if (nByte == LINKUP_EOP)
		{
			stateIn = ReceivePreamble;
			if (pProgressingIn)
			{
				free(pProgressingIn);
			}
		}
		return true;
	}
	return false;
}

void LinkUpClass::send(LinkUpPacket packet)
{
	LinkUpPacketList* pPacketList = (LinkUpPacketList*)calloc(1, sizeof(LinkUpPacketList));
	pPacketList->packet = packet;
	if (pHeadOut != NULL && pTailOut != NULL) {
		pTailIn->next = pPacketList;
		pTailIn = pPacketList;
	}
	else {
		pHeadIn = pTailIn = pPacketList;
	}
}
uint8_t LinkUpClass::getRaw(uint8_t* pData, uint8_t nMax)
{
	return 0;
}

void LinkUpClass::progress(uint8_t *pData, uint8_t nCount) {
	uint8_t i = 0;
	uint8_t nNextByte;

	while (i < nCount)
	{
		switch (stateIn)
		{
		case LinkUpState::ReceivePreamble:
			if (pData[i] == LINKUP_PREAMBLE) {
				skipIn = false;
				stateIn = ReceiveLength;
				pProgressingIn = (LinkUpPacketList*)calloc(1, sizeof(LinkUpPacketList));
			}
			break;
		case LinkUpState::ReceiveLength:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nBytesToRead = pData[i] ^ LINKUP_XOR;
					else
						nBytesToRead = pData[i];

					skipIn = false;

					pProgressingIn->packet.data = (uint8_t*)calloc(nBytesToRead, sizeof(uint8_t));
					pProgressingIn->packet.header.lenght = nBytesToRead;
					stateIn = LinkUpState::ReceiveData;
				}
			}
			break;
		case LinkUpState::ReceiveData:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_XOR;
					else
						nNextByte = pData[i];

					skipIn = false;

					if (nBytesToRead > 0) {
						pProgressingIn->packet.data[pProgressingIn->packet.header.lenght - nBytesToRead] = nNextByte;
						nBytesToRead--;
					}
					if (nBytesToRead <= 0) {
						stateIn = LinkUpState::ReceiveCRC;
					}
				}
			}
			break;
		case LinkUpState::ReceiveCRC:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_XOR;
					else
						nNextByte = pData[i];

					skipIn = false;

					pProgressingIn->packet.crc = ((uint16_t)nNextByte) << 8;
					stateIn = LinkUpState::ReceiveCheckCRC;
				}
			}
			break;
		case LinkUpState::ReceiveCheckCRC:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_XOR;
					else
						nNextByte = pData[i];

					skipIn = false;

					pProgressingIn->packet.crc = pProgressingIn->packet.crc | nNextByte;

					if (pProgressingIn->packet.crc == CRC16.calc(pProgressingIn->packet.data, pProgressingIn->packet.header.lenght))
					{
						stateIn = LinkUpState::ReceiveEnd;
					}
					else
					{
						nTotalFailedPackets++;
						if (pProgressingIn->packet.data)
							free(pProgressingIn->packet.data);
						if (pProgressingIn)
							free(pProgressingIn);
					}
				}
			}
			break;
		case LinkUpState::ReceiveEnd:

			if (pData[i] == LINKUP_EOP)
			{
				nTotalReceivedPackets++;
				if (pHeadIn != NULL && pTailIn != NULL) {
					pTailIn->next = pProgressingIn;
					pTailIn = pProgressingIn;
				}
				else {
					pHeadIn = pTailIn = pProgressingIn;
				}
				stateIn = LinkUpState::ReceivePreamble;
			}
			else {
				nTotalFailedPackets++;
				free(pProgressingIn->packet.data);
				free(pProgressingIn);
			}
			break;
		default:
			break;
		}
		i++;
	}
}

LinkUpClass LinkUp;