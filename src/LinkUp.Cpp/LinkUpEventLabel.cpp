#include "LinkUpEventLabel.h"

uint8_t * LinkUpEventLabel::getOptions(uint8_t* pSize)
{
	*pSize = 0;
	return NULL;
}

void LinkUpEventLabel::fireEvent(uint8_t *pData, uint32_t nSize)
{
	lock();
	if (isSubscribed)
	{
		LinkUpEventData* pEventData = (LinkUpEventData*)calloc(1, sizeof(LinkUpEventData) + nSize);
		pEventData->nSize = nSize;
		if (nSize > 0) {
			memcpy(pEventData->pData, pData, nSize);
		}
		pList->insert(pEventData);
	}
	unlock();
}

LinkUpEventLabel::LinkUpEventLabel(const char* pName, LinkUpNode* pParent)
{
	init(pName, LinkUpLabelType::Event, pParent);
}

void LinkUpEventLabel::progressAdv(LinkUpRaw* pConnector)
{
	lock();
	LinkedListIterator iterator(pList);

	LinkUpEventData* pData;

	while ((pData = (LinkUpEventData*)iterator.next()) != NULL)
	{
		pList->remove(pData);

		LinkUpPacket packet;
		packet.nLength = sizeof(LinkUpLogic) + sizeof(LinkUpEventFireRequest) + pData->nSize;
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* pLogic = (LinkUpLogic*)packet.pData;
		pLogic->nLogicType = LinkUpLogicType::EventFireRequest;
		LinkUpEventFireRequest* pEventFireRequest = (LinkUpEventFireRequest*)pLogic->pInnerHeader;

		pEventFireRequest->nIdentifier = nIdentifier;
		memcpy(pEventFireRequest->pData, pData->pData, pData->nSize);
		if (pData->nSize > 1024)
		{
			pConnector->send(packet, false);
		}
		else
		{
			pConnector->send(packet);
		}

		free(pData);
	}
	unlock();
}

void LinkUpEventLabel::subscribed(LinkUpRaw* pConnector) {
	lock();
	isSubscribed = true;
	unlock();

	LinkUpPacket packet;
	packet.nLength = (uint16_t)sizeof(LinkUpLogic) + (uint16_t)sizeof(LinkUpEventSubscribeResponse);
	packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

	LinkUpLogic* pLogic = (LinkUpLogic*)packet.pData;
	pLogic->nLogicType = LinkUpLogicType::EventSubscribeResponse;
	LinkUpEventSubscribeResponse* pSubscribeResponse = (LinkUpEventSubscribeResponse*)pLogic->pInnerHeader;

	pSubscribeResponse->nIdentifier = nIdentifier;

	pConnector->send(packet);
}

void LinkUpEventLabel::unsubscribed(LinkUpRaw* pConnector) {
	lock();
	isSubscribed = false;
	LinkedListIterator iterator(pList);

	LinkUpEventData* pData;

	while ((pData = (LinkUpEventData*)iterator.next()) != NULL)
	{
		pList->remove(pData);
		free(pData);
	}
	unlock();

	if (pConnector != NULL)
	{
		LinkUpPacket packet;
		packet.nLength = (uint16_t)sizeof(LinkUpLogic) + (uint16_t)sizeof(LinkUpEventSubscribeResponse);
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* pLogic = (LinkUpLogic*)packet.pData;
		pLogic->nLogicType = LinkUpLogicType::EventUnsubscribeResponse;
		LinkUpEventSubscribeResponse* pSubscribeResponse = (LinkUpEventSubscribeResponse*)pLogic->pInnerHeader;

		pSubscribeResponse->nIdentifier = nIdentifier;

		pConnector->send(packet);
	}
}