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
#ifdef _WINDOWS | __linux
	uint32_t nTime = (uint32_t)1000 * 1000 / CLOCKS_PER_SEC * clock();
#else
	uint32_t nTime = micros();
#endif // _WINDOWS || __linux

	connector.progress(pData, nCount);

	uint16_t i = 0;

	while (connector.hasNext() && nMax > i) {
		i++;
		LinkUpPacket packet = connector.next();
		receivedPacket(packet);
	}

	if (!isInitialized && nTime > timestamps.nInitTryTimeout && pName != NULL) {
		timestamps.nInitTryTimeout = nTime + INITIALIZATION_TIMEOUT;

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
		AvlTreeIterator iterator(pAvlTree);
		AvlNode* pNode;
		while ((pNode = iterator.next()) != NULL)
		{
			if (pNode->pData != NULL)
			{
				((LinkUpLabel*)pNode->pData)->progress(&connector);
			}
		}

		LinkUpLabelList* pCurrent = pHead;

		while (pCurrent != 0) {
			pCurrent->pLabel->progress(&connector);
			pCurrent = pCurrent->pNext;
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
	else {

		/*AvlTreeIterator iterator(pAvlTree);
		AvlNode* pNode;
		while ((pNode = iterator.next()) != NULL)
		{
			if (pNode->pData != NULL)
			{
				if (((LinkUpLabel*)pNode->pData)->receivedNameResponse(pResponseName, pNameResponse->nLabelType, pNameResponse->nIdentifier)) {
					break;
				}
			}
		}*/

		LinkUpLabelList* pCurrent = pHead;
		LinkUpLabelList* pPrev = pHead;
		while (pCurrent != 0) {
			if (pCurrent->pLabel->receivedNameResponse(pResponseName, pNameResponse->nLabelType, pNameResponse->nIdentifier))
			{
				pAvlTree->insert(pNameResponse->nIdentifier, pCurrent->pLabel);

				//cout << " ID: " << pNameResponse->nIdentifier << " Name: " << pResponseName << endl;

				if (pCurrent == pHead)
				{
					pHead = pCurrent->pNext;
				}
				else
				{
					pPrev->pNext = pCurrent->pNext;
				}
				free(pCurrent);
				pCurrent = 0;
			}
			else
			{
				pPrev = pCurrent;
				pCurrent = pCurrent->pNext;
			}
		}
	}
	free(pResponseName);
}

void LinkUpNode::receivedPropertyGetRequest(LinkUpPacket packet, LinkUpPropertyGetRequest* pPropertyGetRequest)
{
	/*AvlTreeIterator iterator(pAvlTree);
	AvlNode* pNode;
	while ((pNode = iterator.next()) != NULL)
	{
		if (pNode->pData != NULL)
		{
			if (((LinkUpLabel*)pNode->pData)->receivedPropertyGetRequest(pPropertyGetRequest->nIdentifier, &connector)) {
				break;
			}
		}
	}
	while (iterator.next()) {}*/
	if (pAvlTree != NULL && pPropertyGetRequest != NULL) {
		AvlNode* pNode = pAvlTree->find(pPropertyGetRequest->nIdentifier);
		if (pNode != NULL && pNode->pData != NULL) {
			LinkUpLabel* label = (LinkUpLabel*)pNode->pData;
			if (!label->receivedPropertyGetRequest(pPropertyGetRequest->nIdentifier, &connector)) {
				//TODO: error??
				int a = 0;
			}
		}
		else {
			int a = 0;
		}
	}

	/*LinkUpLabelList* pCurrent = pHead;
	while (pCurrent != 0)
	{
		if (pCurrent->pLabel->receivedPropertyGetRequest(pPropertyGetRequest->nIdentifier, &connector))
		{
			pCurrent = 0;
		}
		else
		{
			pCurrent = pCurrent->pNext;
		}
	}*/
}

void LinkUpNode::receivedPropertyGetResponse(LinkUpPacket packet, LinkUpPropertyGetResponse* pPropertyGetResponse) {
}

void LinkUpNode::receivedPropertySetRequest(LinkUpPacket packet, LinkUpPropertySetRequest* pPropertySetRequest)
{
	AvlTreeIterator iterator(pAvlTree);
	AvlNode* pNode;
	while ((pNode = iterator.next()) != NULL)
	{
		if (pNode->pData != NULL)
		{
			if (((LinkUpLabel*)pNode->pData)->receivedPropertySetRequest(pPropertySetRequest->nIdentifier, pPropertySetRequest->pData, &connector)) {
				break;
			}
		}
	}
	while (iterator.next()) {}
	//TODO: us tree-find();

	/*LinkUpLabelList* pCurrent = pHead;
	while (pCurrent != 0)
	{
		if (pCurrent->pLabel->receivedPropertySetRequest(pPropertySetRequest->nIdentifier, pPropertySetRequest->pData, &connector))
		{
			pCurrent = 0;
		}
		else
		{
			pCurrent = pCurrent->pNext;
		}
	}*/
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
	LinkUpLabel* pLabel = new LinkUpLabel();
	LinkUpLabelList* pCurrent = (LinkUpLabelList*)calloc(1, sizeof(LinkUpLabelList));

	pLabel->init(pName, type);
	pCurrent->pLabel = pLabel;
	pCurrent->pNext = pHead;
	pHead = pCurrent;

	return pLabel;
}

void LinkUpLabel::init(const char* pName, LinkUpLabelType type)
{
	this->pName = (char*)calloc(strlen(pName) + 1, sizeof(uint8_t));
	strcpy(this->pName, pName);
	this->type = type;

	switch (type)
	{
	case Node:
		nSize = 0;
		break;
	case Event:
		nSize = 0;
		break;
	case Function:
		nSize = 0;
		break;
	case Boolean:
		nSize = 1;
		break;
	case SByte:
		nSize = 1;
		break;
	case Byte:
		nSize = 1;
		break;
	case Int16:
		nSize = 2;
		break;
	case UInt16:
		nSize = 2;
		break;
	case Int32:
		nSize = 4;
		break;
	case UInt32:
		nSize = 4;
		break;
	case Int64:
		nSize = 8;
		break;
	case UInt64:
		nSize = 8;
		break;
	case Single:
		nSize = 4;
		break;
	case Double:
		nSize = 8;
		break;
	default:
		break;
	}
	if (nSize > 0) {
		pValue = calloc(nSize, sizeof(uint8_t));
	}
}

void LinkUpLabel::set(void* pValue)
{
	memcpy(this->pValue, pValue, nSize);
}

void* LinkUpLabel::get()
{
	return pValue;
}

void LinkUpLabel::progress(LinkUpRaw* pConnector) {
#ifdef _WINDOWS | __linux
	uint32_t nTime = (uint32_t)1000 * 1000 / CLOCKS_PER_SEC * clock();
#else
	uint32_t nTime = micros();
#endif // _WINDOWS || __linux

	if (!isInitialized && nTime > timestamps.nInitTryTimeout && pName != NULL) {
		timestamps.nInitTryTimeout = nTime + INITIALIZATION_TIMEOUT;

		LinkUpPacket packet;
		packet.nLength = strlen(pName) + sizeof(LinkUpLogic) + sizeof(LinkUpNameRequest);
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		LinkUpNameRequest* nameRequest = (LinkUpNameRequest*)logic->pInnerHeader;

		logic->nLogicType = LinkUpLogicType::NameRequest;
		nameRequest->nLabelType = type;
		memcpy(nameRequest->pName, pName, strlen(pName));

		pConnector->send(packet);
	}
}

bool LinkUpLabel::receivedPropertySetRequest(uint16_t nIdentifier, uint8_t* pValue, LinkUpRaw* pConnector)
{
	if (this->nIdentifier == nIdentifier)
	{
		LinkUpPacket packet;
		packet.nLength = nSize + sizeof(LinkUpLogic) + sizeof(LinkUpPropertySetResponse);
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		LinkUpPropertySetResponse* setResponse = (LinkUpPropertySetResponse*)logic->pInnerHeader;

		logic->nLogicType = LinkUpLogicType::PropertySetResponse;
		setResponse->nIdentifier = nIdentifier;

		memcpy(this->pValue, pValue, nSize);

		pConnector->send(packet);
		return true;
	}
	else
	{
		return false;
	}
}

bool LinkUpLabel::receivedNameResponse(const char* pName, LinkUpLabelType type, uint16_t nIdentifier) {
	if (strcmp(this->pName, pName) == 0 && this->type == type)
	{
		isInitialized = true;
		this->nIdentifier = nIdentifier;
		return true;
	}
	else
	{
		return false;
	}
}

char* LinkUpLabel::getName() {
	return pName;
}

bool LinkUpLabel::receivedPropertyGetRequest(uint16_t nIdentifier, LinkUpRaw* pConnector)
{
	if (this->nIdentifier == nIdentifier)
	{
		LinkUpPacket packet;
		packet.nLength = nSize + sizeof(LinkUpLogic) + sizeof(LinkUpPropertyGetResponse);
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		LinkUpPropertyGetResponse* getResponse = (LinkUpPropertyGetResponse*)logic->pInnerHeader;

		logic->nLogicType = LinkUpLogicType::PropertyGetResponse;
		getResponse->nIdentifier = nIdentifier;
		memcpy(getResponse->pData, pValue, nSize);

		pConnector->send(packet);
		return true;
	}
	else
	{
		return false;
	}
}