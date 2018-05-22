#include "LinkUpLabel.h"

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
	uint32_t nTime = getSystemTime();

	if (!isInitialized && nTime > timestamps.nInitTryTimeout && pName != NULL) {
		timestamps.nInitTryTimeout = nTime + initialization_timeout;

		LinkUpPacket packet;
		packet.nLength = strlen(pName) + sizeof(LinkUpLogic) + sizeof(LinkUpNameRequest);
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		LinkUpNameRequest* nameRequest = (LinkUpNameRequest*)logic->pInnerHeader;

		logic->nLogicType = LinkUpLogicType::NameRequest;
		nameRequest->nLabelType = type;
		nameRequest->nNameLength = strlen(pName);
		memcpy(nameRequest->pName, pName, nameRequest->nNameLength);

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