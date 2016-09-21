/**
* Author: Thomas Weichselbaumer
* Version: 0.1.0
* File Name: LinkUp.cpp
* Description: Source file for the LinkUp lib.
**/

#include "LinkUp.h"

LinkUpPacket LinkUpClass::next()
{
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

bool LinkUpClass::hasNext()
{
	return pHeadIn != NULL;
}

bool LinkUpClass::checkForError(uint8_t nByte)
{
	if (nByte == LINKUP_EOP || nByte == LINKUP_PREAMBLE)
	{
		nTotalFailedPackets++;

		if (pProgressingIn)
		{
			if (pProgressingIn->packet.pData)
			{
				free(pProgressingIn->packet.pData);
			}
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
	if (packet.nLenght)
	{
		LinkUpPacketList* pPacketList = (LinkUpPacketList*)calloc(1, sizeof(LinkUpPacketList));
		pPacketList->packet = packet;

		pPacketList->packet.nCrc = CRC16.calc(pPacketList->packet.pData, pPacketList->packet.nLenght);

		if (pHeadOut != NULL && pTailOut != NULL)
		{
			pTailOut->next = pPacketList;
			pTailOut = pPacketList;
		}
		else
		{
			pHeadOut = pTailOut = pPacketList;
		}
	}
}

uint8_t LinkUpClass::getRaw(uint8_t* pData, uint8_t nMax)
{
	uint8_t nBytesSend = 0;
	uint8_t nNextByte = 0;
	do
	{
		switch (stateOut)
		{
		case LinkUpState::SendIdle:
			if (pProgressingOut)
			{
				if (pProgressingOut->packet.pData)
					free(pProgressingOut->packet.pData);
				free(pProgressingOut);
			}
			if (pHeadOut != NULL)
			{
				pProgressingOut = pHeadOut;
				pHeadOut = pProgressingOut->next;
				stateOut = LinkUpState::SendPreamble;
				nBytesToSend = pProgressingOut->packet.nLenght;
			}
			break;
		case LinkUpState::SendPreamble:
			pData[nBytesSend] = LINKUP_PREAMBLE;
			nBytesSend++;
			stateOut = LinkUpState::SendLenght;
			break;
		case LinkUpState::SendLenght:
			nNextByte = nBytesToSend;
			if ((nNextByte == LINKUP_PREAMBLE || nNextByte == LINKUP_EOP || nNextByte == LINKUP_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte ^ LINKUP_XOR;
				}
				else
				{
					pData[nBytesSend] = nNextByte;
				}
				skipOut = false;
				stateOut = LinkUpState::SendData;
			}
			nBytesSend++;
			break;
		case LinkUpState::SendData:
			if (nBytesToSend > 0)
			{
				nNextByte = pProgressingOut->packet.pData[pProgressingOut->packet.nLenght - nBytesToSend];
				if ((nNextByte == LINKUP_PREAMBLE || nNextByte == LINKUP_EOP || nNextByte == LINKUP_SKIP) && !skipOut)
				{
					skipOut = true;
					pData[nBytesSend] = LINKUP_SKIP;
				}
				else
				{
					if (skipOut)
					{
						pData[nBytesSend] = nNextByte ^ LINKUP_XOR;
					}
					else
					{
						pData[nBytesSend] = nNextByte;
					}
					skipOut = false;
					nBytesToSend--;
				}
				nBytesSend++;
			}
			else
			{
				stateOut = LinkUpState::SendCrc1;
			}
			break;
		case LinkUpState::SendCrc1:
			nNextByte = (pProgressingOut->packet.nCrc & 0x00ff);
			if ((nNextByte == LINKUP_PREAMBLE || nNextByte == LINKUP_EOP || nNextByte == LINKUP_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte^ LINKUP_XOR;
				}
				else
				{
					pData[nBytesSend] = nNextByte;
				}
				skipOut = false;
				stateOut = LinkUpState::SendCrc2;
			}
			nBytesSend++;
			break;
		case LinkUpState::SendCrc2:
			nNextByte = (pProgressingOut->packet.nCrc & 0xff00) >> 8;
			if ((nNextByte == LINKUP_PREAMBLE || nNextByte == LINKUP_EOP || nNextByte == LINKUP_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte ^ LINKUP_XOR;
				}
				else
				{
					pData[nBytesSend] = nNextByte;
				}
				skipOut = false;
				stateOut = LinkUpState::SendEnd;
			}
			nBytesSend++;
			break;
		case LinkUpState::SendEnd:
			pData[nBytesSend] = LINKUP_EOP;
			nBytesSend++;
			stateOut = LinkUpState::SendIdle;
			break;
		default:
			break;
		}
	} while (nBytesSend <= nMax && stateOut != LinkUpState::SendIdle);

	return nBytesSend;
}

void LinkUpClass::progress(uint8_t *pData, uint8_t nCount)
{
	uint8_t i = 0;
	uint8_t nNextByte;

	while (i < nCount)
	{
		switch (stateIn)
		{
		case LinkUpState::ReceivePreamble:
			if (pData[i] == LINKUP_PREAMBLE)
			{
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
					if (nBytesToRead > 0)
					{
						pProgressingIn->packet.pData = (uint8_t*)calloc(nBytesToRead, sizeof(uint8_t));
						pProgressingIn->packet.nLenght = nBytesToRead;
						stateIn = LinkUpState::ReceiveData;
					}
					else
					{
						if (pProgressingIn)
							free(pProgressingIn);
						nTotalFailedPackets++;
						stateIn = LinkUpState::SendPreamble;
					}
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
						pProgressingIn->packet.pData[pProgressingIn->packet.nLenght - nBytesToRead] = nNextByte;
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

					pProgressingIn->packet.nCrc = ((uint16_t)nNextByte);
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

					pProgressingIn->packet.nCrc = pProgressingIn->packet.nCrc | (nNextByte << 8);

					if (pProgressingIn->packet.nCrc == CRC16.calc(pProgressingIn->packet.pData, pProgressingIn->packet.nLenght))
					{
						stateIn = LinkUpState::ReceiveEnd;
					}
					else
					{
						nTotalFailedPackets++;

						if (pProgressingIn)
						{
							if (pProgressingIn->packet.pData)
								free(pProgressingIn->packet.pData);
							free(pProgressingIn);
						}
					}
				}
			}
			break;
		case LinkUpState::ReceiveEnd:
			if (pData[i] == LINKUP_EOP)
			{
				nTotalReceivedPackets++;
				if (pHeadIn != NULL && pTailIn != NULL)
				{
					pTailIn->next = pProgressingIn;
					pTailIn = pProgressingIn;
				}
				else
				{
					pHeadIn = pTailIn = pProgressingIn;
				}
				stateIn = LinkUpState::ReceivePreamble;
			}
			else
			{
				nTotalFailedPackets++;
				if (pProgressingIn)
				{
					if (pProgressingIn->packet.pData)
						free(pProgressingIn->packet.pData);
					free(pProgressingIn);
				}
			}
			break;
		default:
			break;
		}
		i++;
	}
}

LinkUpClass LinkUp;