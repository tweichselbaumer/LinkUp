/**
* Author: Thomas Weichselbaumer
* Version: 1.0.0
* File Name: LinkUp.cpp
* Description: Source file for the LinkUp lib.
**/

#include "LinkUpRaw.h"

LinkUpPacket LinkUpRaw::next()
{
	lock_queue();
	LinkUpPacketList *pPacketList;
	LinkUpPacket packet;
	pPacketList = pHeadIn;
	if (pPacketList != NULL) {
		pHeadIn = pPacketList->next;
		packet = pPacketList->packet;
		free(pPacketList);
	}
	unlock_queue();
	return packet;
}

void LinkUpRaw::lock_queue()
{
#ifdef LINKUP_BOOST_THREADSAFE
	mtx_queue.lock();
#endif
}

void LinkUpRaw::unlock_queue()
{
#ifdef LINKUP_BOOST_THREADSAFE
	mtx_queue.unlock();
#endif
}

void LinkUpRaw::lock_progressing()
{
#ifdef LINKUP_BOOST_THREADSAFE
	mtx_progressing.lock();
#endif
}

void LinkUpRaw::unlock_progressing()
{
#ifdef LINKUP_BOOST_THREADSAFE
	mtx_progressing.unlock();
#endif
}


bool LinkUpRaw::hasNext()
{
	return pHeadIn != NULL;
}

bool LinkUpRaw::checkForError(uint8_t nByte)
{
	if (nByte == LINKUP_RAW_EOP || nByte == LINKUP_RAW_PREAMBLE)
	{
		nTotalFailedPackets++;

		if (pProgressingIn)
		{
			if (pProgressingIn->packet.pData)
			{
				free(pProgressingIn->packet.pData);
			}
		}

		if (nByte == LINKUP_RAW_PREAMBLE)
		{
			stateIn = ReceiveLength1;
		}

		if (nByte == LINKUP_RAW_EOP)
		{
			stateIn = ReceivePreamble;
			if (pProgressingIn)
			{
				free(pProgressingIn);
			}
		}
		stateIn = LinkUpState::ReceivePreamble;
		return true;
	}
	return false;
}

void LinkUpRaw::send(LinkUpPacket packet)
{
	send(packet, true);
}

void LinkUpRaw::send(LinkUpPacket packet, bool bWithCrc)
{
	if (packet.nLength)
	{
		LinkUpPacketList* pPacketList = (LinkUpPacketList*)calloc(1, sizeof(LinkUpPacketList));
		pPacketList->packet = packet;

		if (bWithCrc)
		{
			pPacketList->packet.nCrc = CRC16::calc(pPacketList->packet.pData, pPacketList->packet.nLength);
		}
		else
		{
			pPacketList->packet.nCrc = 0;
		}


		lock_queue();
		if (pHeadOut != NULL)
		{
			pTailOut->next = pPacketList;
			pTailOut = pPacketList;
		}
		else
		{
			pHeadOut = pTailOut = pPacketList;
		}
		unlock_queue();
	}
}

uint16_t LinkUpRaw::getRaw(uint8_t* pData, uint32_t nMax)
{
	uint32_t nBytesSend = 0;
	uint8_t nNextByte = 0;
	lock_progressing();
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
				pProgressingOut = NULL;
			}
			if (pHeadOut != NULL)
			{
				lock_queue();
				nTotalSendPackets++;
				pProgressingOut = pHeadOut;
				pHeadOut = pProgressingOut->next;
				stateOut = LinkUpState::SendPreamble;
				nBytesToSend = pProgressingOut->packet.nLength;
				unlock_queue();
			}
			break;
		case LinkUpState::SendPreamble:
			pData[nBytesSend] = LINKUP_RAW_PREAMBLE;
			nBytesSend++;
			stateOut = LinkUpState::SendLength1;
			break;
		case LinkUpState::SendLength1:
			nNextByte = (nBytesToSend & 0x000000ff);
			if ((nNextByte == LINKUP_RAW_PREAMBLE || nNextByte == LINKUP_RAW_EOP || nNextByte == LINKUP_RAW_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_RAW_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte ^ LINKUP_RAW_XOR;
				}
				else
				{
					pData[nBytesSend] = nNextByte;
				}
				skipOut = false;
				stateOut = LinkUpState::SendLength2;
			}
			nBytesSend++;
			break;
		case LinkUpState::SendLength2:
			nNextByte = (nBytesToSend & 0x0000ff00) >> 8;
			if ((nNextByte == LINKUP_RAW_PREAMBLE || nNextByte == LINKUP_RAW_EOP || nNextByte == LINKUP_RAW_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_RAW_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte ^ LINKUP_RAW_XOR;
				}
				else
				{
					pData[nBytesSend] = nNextByte;
				}
				skipOut = false;
				stateOut = LinkUpState::SendLength3;
			}
			nBytesSend++;
			break;
		case LinkUpState::SendLength3:
			nNextByte = (nBytesToSend & 0x00ff0000) >> 16;
			if ((nNextByte == LINKUP_RAW_PREAMBLE || nNextByte == LINKUP_RAW_EOP || nNextByte == LINKUP_RAW_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_RAW_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte ^ LINKUP_RAW_XOR;
				}
				else
				{
					pData[nBytesSend] = nNextByte;
				}
				skipOut = false;
				stateOut = LinkUpState::SendLength4;
			}
			nBytesSend++;
			break;
		case LinkUpState::SendLength4:
			nNextByte = (nBytesToSend & 0xff000000) >> 24;
			if ((nNextByte == LINKUP_RAW_PREAMBLE || nNextByte == LINKUP_RAW_EOP || nNextByte == LINKUP_RAW_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_RAW_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte ^ LINKUP_RAW_XOR;
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
				nNextByte = pProgressingOut->packet.pData[pProgressingOut->packet.nLength - nBytesToSend];
				if ((nNextByte == LINKUP_RAW_PREAMBLE || nNextByte == LINKUP_RAW_EOP || nNextByte == LINKUP_RAW_SKIP) && !skipOut)
				{
					skipOut = true;
					pData[nBytesSend] = LINKUP_RAW_SKIP;
				}
				else
				{
					if (skipOut)
					{
						pData[nBytesSend] = nNextByte ^ LINKUP_RAW_XOR;
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
			if ((nNextByte == LINKUP_RAW_PREAMBLE || nNextByte == LINKUP_RAW_EOP || nNextByte == LINKUP_RAW_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_RAW_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte ^ LINKUP_RAW_XOR;
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
			if ((nNextByte == LINKUP_RAW_PREAMBLE || nNextByte == LINKUP_RAW_EOP || nNextByte == LINKUP_RAW_SKIP) && !skipOut)
			{
				skipOut = true;
				pData[nBytesSend] = LINKUP_RAW_SKIP;
			}
			else
			{
				if (skipOut)
				{
					pData[nBytesSend] = nNextByte ^ LINKUP_RAW_XOR;
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
			pData[nBytesSend] = LINKUP_RAW_EOP;
			nBytesSend++;
			stateOut = LinkUpState::SendIdle;
			break;
		default:
			break;
		}
	} while (nBytesSend < nMax && (stateOut != LinkUpState::SendIdle || pHeadOut != NULL));

	if (nBytesSend > 0) {
		nTotalSendBytes += nBytesSend;
		//std::cout << "p rec " << nTotalReceivedPackets << " p f: " << nTotalFailedPackets << " p sen: " << nTotalSendPackets << " brec: " << nTotalReceivedBytes << " bsen: " << nTotalSendBytes << std::endl;
	}
	unlock_progressing();
	return nBytesSend;
}

void LinkUpRaw::progress(uint8_t *pData, uint32_t nCount)
{
	uint32_t i = 0;
	uint8_t nNextByte;

	nTotalReceivedBytes += nCount;

	while (i < nCount)
	{
		switch (stateIn)
		{
		case LinkUpState::ReceivePreamble:
			if (pData[i] == LINKUP_RAW_PREAMBLE)
			{
				skipIn = false;
				stateIn = ReceiveLength1;
				pProgressingIn = (LinkUpPacketList*)calloc(1, sizeof(LinkUpPacketList));
			}
			break;
		case LinkUpState::ReceiveLength1:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_RAW_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_RAW_XOR;
					else
						nNextByte = pData[i];

					stateIn = LinkUpState::ReceiveLength2;

					skipIn = false;

					pProgressingIn->packet.nLength = nNextByte;
				}
			}
			break;
		case LinkUpState::ReceiveLength2:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_RAW_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_RAW_XOR;
					else
						nNextByte = pData[i];

					stateIn = LinkUpState::ReceiveLength3;

					skipIn = false;

					pProgressingIn->packet.nLength |= (nNextByte << 8);
				}
			}
			break;
		case LinkUpState::ReceiveLength3:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_RAW_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_RAW_XOR;
					else
						nNextByte = pData[i];

					stateIn = LinkUpState::ReceiveLength4;

					skipIn = false;

					pProgressingIn->packet.nLength |= (nNextByte << 16);
				}
			}
			break;
		case LinkUpState::ReceiveLength4:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_RAW_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_RAW_XOR;
					else
						nNextByte = pData[i];

					skipIn = false;

					pProgressingIn->packet.nLength |= (nNextByte << 24);

					nBytesToRead = pProgressingIn->packet.nLength;

					if (pProgressingIn->packet.nLength > 0)
					{
						pProgressingIn->packet.pData = (uint8_t*)calloc(pProgressingIn->packet.nLength, sizeof(uint8_t));
						stateIn = LinkUpState::ReceiveData;
					}
					else
					{
						if (pProgressingIn)
							free(pProgressingIn);
						nTotalFailedPackets++;
						stateIn = LinkUpState::ReceivePreamble;
					}
				}
			}
			break;
		case LinkUpState::ReceiveData:
			if (!checkForError(pData[i]))
			{
				if (pData[i] == LINKUP_RAW_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_RAW_XOR;
					else
						nNextByte = pData[i];

					skipIn = false;

					if (nBytesToRead > 0) {
						pProgressingIn->packet.pData[pProgressingIn->packet.nLength - nBytesToRead] = nNextByte;
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
				if (pData[i] == LINKUP_RAW_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_RAW_XOR;
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
				if (pData[i] == LINKUP_RAW_SKIP)
				{
					skipIn = true;
				}
				else
				{
					if (skipIn)
						nNextByte = pData[i] ^ LINKUP_RAW_XOR;
					else
						nNextByte = pData[i];

					skipIn = false;

					pProgressingIn->packet.nCrc = pProgressingIn->packet.nCrc | (nNextByte << 8);

					if (pProgressingIn->packet.nCrc == CRC16::calc(pProgressingIn->packet.pData, pProgressingIn->packet.nLength))
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
						stateIn = LinkUpState::ReceivePreamble;
					}
				}
			}
			break;
		case LinkUpState::ReceiveEnd:
			if (pData[i] == LINKUP_RAW_EOP)
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

	if (nCount > 0) {
		//std::cout << "p rec " << nTotalReceivedPackets << " p f: " << nTotalFailedPackets << " p sen: " << nTotalSendPackets << " brec: " << nTotalReceivedBytes << " bsen: " << nTotalSendBytes << std::endl;
	}
}