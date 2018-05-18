#include "LinkUpNode.h"

uint16_t LinkUpNode::getRaw(uint8_t* pData, uint16_t nMax)
{
	uint16_t nResult;
	lock();
	nResult = connector.getRaw(pData, nMax);
	unlock();
	return nResult;
}

void LinkUpNode::lock()
{
#ifdef LINKUP_BOOST_THREADSAFE
	mtx.lock();
#endif
}

void LinkUpNode::unlock()
{
#ifdef LINKUP_BOOST_THREADSAFE
	mtx.unlock();
#endif
}

void LinkUpLabel::lock()
{
#ifdef LINKUP_BOOST_THREADSAFE
	mtx.lock();
#endif
}

void LinkUpLabel::unlock()
{
#ifdef LINKUP_BOOST_THREADSAFE
	mtx.unlock();
#endif
}

void LinkUpNode::progress(uint8_t* pData, uint16_t nCount, uint16_t nMax)
{
	lock();

	uint32_t nTime = getSystemTime();

	connector.progress(pData, nCount);

	uint16_t i = 0;

	while (connector.hasNext() && nMax > i) {
		i++;
		LinkUpPacket packet = connector.next();
		receivedPacket(packet);
	}

	if (!isInitialized && nTime > timestamps.nInitTryTimeout && pName != NULL) {
		timestamps.nInitTryTimeout = nTime + initialization_timeout;

		LinkUpPacket packet;
		packet.nLength = strlen(pName) + sizeof(LinkUpLogic) + sizeof(LinkUpNameRequest);
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		LinkUpNameRequest* nameRequest = (LinkUpNameRequest*)logic->pInnerHeader;

		logic->nLogicType = LinkUpLogicType::NameRequest;
		nameRequest->nLabelType = LinkUpLabelType::Node;
		memcpy(nameRequest->pName, pName, strlen(pName));

		connector.send(packet);
	}
	else if (isInitialized)
	{
		LinkedListIterator iterator(pList);
		LinkUpLabel* pLabel;

		while ((pLabel = (LinkUpLabel*)iterator.next()) != NULL)
		{
			pLabel->progress(&connector);
		}
	}
	unlock();
}

void LinkUpNode::init(const char* pName)
{
	this->pName = (char*)calloc(strlen(pName) + 1, sizeof(uint8_t));
	strcpy(this->pName, pName);
}

void LinkUpNode::receivedPacket(LinkUpPacket packet)
{
	if (packet.nLength > 0 && packet.pData != NULL) {
		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		switch (logic->nLogicType)
		{
		case LinkUpLogicType::NameRequest:
			receivedNameRequest(packet, (LinkUpNameRequest*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::NameResponse:
			receivedNameResponse(packet, (LinkUpNameResponse*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PropertyGetRequest:
			receivedPropertyGetRequest(packet, (LinkUpPropertyGetRequest*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PropertyGetResponse:
			receivedPropertyGetResponse(packet, (LinkUpPropertyGetResponse*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PropertySetRequest:
			receivedPropertySetRequest(packet, (LinkUpPropertySetRequest*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PropertySetResponse:
			receivedPropertySetResponse(packet, (LinkUpPropertySetResponse*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PingRequest:
			receivedPingRequest(packet);
			break;
		default:
			break;
		}
	}

	if (packet.pData != NULL) {
		free(packet.pData);
		packet.pData = NULL;
	}
}

void LinkUpNode::receivedNameRequest(LinkUpPacket packet, LinkUpNameRequest* pNameRequest) {
}

void LinkUpNode::receivedNameResponse(LinkUpPacket packet, LinkUpNameResponse* pNameResponse) {
	char *pResponseName;
	uint32_t nLength = packet.nLength - sizeof(LinkUpLogic) - sizeof(LinkUpNameResponse);

	pResponseName = (char*)calloc(nLength + 1, sizeof(uint8_t));
	memcpy(pResponseName, pNameResponse->pName, nLength);

	if (pNameResponse->nLabelType == LinkUpLabelType::Node && strcmp(pName, pResponseName) == 0)
	{
		nIdentifier = pNameResponse->nIdentifier;
		isInitialized = true;
	}
	else
	{
		LinkedListIterator iterator(pList);
		LinkUpLabel* pLabel;

		while ((pLabel = (LinkUpLabel*)iterator.next()) != NULL)
		{
			if (pLabel->receivedNameResponse(pResponseName, pNameResponse->nLabelType, pNameResponse->nIdentifier)) {
				pAvlTree->insert(pNameResponse->nIdentifier, pLabel);
				pList->remove(pLabel);
				//cout << " ID: " << pNameResponse->nIdentifier << " Name: " << pResponseName << endl;
			}
		}
	}
	free(pResponseName);
}

void LinkUpNode::receivedPropertyGetRequest(LinkUpPacket packet, LinkUpPropertyGetRequest* pPropertyGetRequest)
{
	if (pAvlTree != NULL && pPropertyGetRequest != NULL) {
		AvlNode* pNode = pAvlTree->find(pPropertyGetRequest->nIdentifier);
		if (pNode != NULL && pNode->pData != NULL) {
			LinkUpLabel* label = (LinkUpLabel*)pNode->pData;
			if (!label->receivedPropertyGetRequest(pPropertyGetRequest->nIdentifier, &connector)) {
				//TODO: error??
			}
		}
		else {
			//TODO: error??
		}
	}
}

void LinkUpNode::receivedPropertyGetResponse(LinkUpPacket packet, LinkUpPropertyGetResponse* pPropertyGetResponse) {
}

void LinkUpNode::receivedPropertySetRequest(LinkUpPacket packet, LinkUpPropertySetRequest* pPropertySetRequest)
{
	if (pAvlTree != NULL && pPropertySetRequest != NULL) {
		AvlNode* pNode = pAvlTree->find(pPropertySetRequest->nIdentifier);
		if (pNode != NULL && pNode->pData != NULL) {
			LinkUpLabel* label = (LinkUpLabel*)pNode->pData;
			if (!label->receivedPropertySetRequest(pPropertySetRequest->nIdentifier, pPropertySetRequest->pData, &connector)) {
				//TODO: error??
			}
		}
		else {
			//TODO: error??
		}
	}
}

void LinkUpNode::receivedPropertySetResponse(LinkUpPacket packet, LinkUpPropertySetResponse* pPropertySetResponse) {
}

void LinkUpNode::receivedPingRequest(LinkUpPacket packet)
{
	LinkUpPacket response;
	response.nLength = sizeof(LinkUpLogic);
	response.pData = (uint8_t*)calloc(response.nLength, sizeof(uint8_t));

	LinkUpLogic* logic = (LinkUpLogic*)response.pData;
	LinkUpPropertyGetResponse* getResponse = (LinkUpPropertyGetResponse*)logic->pInnerHeader;

	logic->nLogicType = LinkUpLogicType::PingResponse;

	connector.send(response);
}

LinkUpLabel* LinkUpNode::addLabel(const char* pName, LinkUpLabelType type)
{
	lock();

	LinkUpLabel* pLabel = new LinkUpLabel();
	pList->insert(pLabel);
	pLabel->init(pName, type);

	unlock();
	return pLabel;
}