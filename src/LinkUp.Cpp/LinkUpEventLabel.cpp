#include "LinkUpEventLabel.h"

uint8_t * LinkUpEventLabel::getOptions(uint8_t* pSize)
{
	return NULL;
}

void LinkUpEventLabel::fireEvent(uint8_t *pData, uint16_t nSize)
{
	if (isSubscribed)
	{
		LinkUpEventData* pEventData = (LinkUpEventData*)calloc(1, sizeof(LinkUpEventData) + nSize);
		pEventData->nSize = nSize;
		if (nSize > 0) {
			memcpy(pEventData->pData, pData, nSize);
		}
		pList->insert(pEventData);
	}
}

LinkUpEventLabel::LinkUpEventLabel(const char* pName, LinkUpNode* pParent)
{
	init(pName, LinkUpLabelType::Event, pParent);
}

void LinkUpEventLabel::progressAdv(LinkUpRaw* pConnector)
{
	LinkedListIterator iterator(pList);
	LinkUpEventData* pData;

	while ((pData = (LinkUpEventData*)iterator.next()) != NULL)
	{
		pList->remove(pData);

		LinkUpPacket packet;
		packet.nLength = (uint16_t)sizeof(LinkUpLogic) + (uint16_t)sizeof(LinkUpEventFireRequest) + pData->nSize;
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* pLogic = (LinkUpLogic*)packet.pData;
		pLogic->nLogicType = LinkUpLogicType::EventFireRequest;
		LinkUpEventFireRequest* pEventFireRequest = (LinkUpEventFireRequest*)pLogic->pInnerHeader;

		pEventFireRequest->nIdentifier = nIdentifier;
		memcpy(pEventFireRequest->pData, pData->pData, pData->nSize);

		pConnector->send(packet);

		free(pData);
	}
}

void LinkUpEventLabel::subscribed(LinkUpRaw* pConnector) {
	isSubscribed = true;

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
	isSubscribed = false;
	LinkedListIterator iterator(pList);

	LinkUpEventData* pData;

	while ((pData = (LinkUpEventData*)iterator.next()) != NULL)
	{
		pList->remove(pData);
		free(pData);
	}

	LinkUpPacket packet;
	packet.nLength = (uint16_t)sizeof(LinkUpLogic) + (uint16_t)sizeof(LinkUpEventSubscribeResponse);
	packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

	LinkUpLogic* pLogic = (LinkUpLogic*)packet.pData;
	pLogic->nLogicType = LinkUpLogicType::EventUnsubscribeResponse;
	LinkUpEventSubscribeResponse* pSubscribeResponse = (LinkUpEventSubscribeResponse*)pLogic->pInnerHeader;

	pSubscribeResponse->nIdentifier = nIdentifier;

	pConnector->send(packet);
}