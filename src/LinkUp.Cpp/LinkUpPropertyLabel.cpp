#include "LinkUpPropertyLabel.h"

bool LinkUpPropertyLabel::receivedPropertyGetRequest(uint16_t nIdentifier, LinkUpRaw* pConnector)
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
		memcpy(getResponse->pData, getRaw(), nSize);

		pConnector->send(packet);
		return true;
	}
	else
	{
		return false;
	}
}

bool LinkUpPropertyLabel::receivedPropertySetRequest(uint16_t nIdentifier, uint8_t* pValue, LinkUpRaw* pConnector)
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

		memcpy(getRaw(), pValue, nSize);

		pConnector->send(packet);
		return true;
	}
	else
	{
		return false;
	}
}

void LinkUpPropertyLabel::init(const char* pName, LinkUpPropertyType nType, uint16_t nSize) {
	this->nType = nType;
	this->nSize = nSize;
	LinkUpLabel::init(pName, LinkUpLabelType::Property);
}

uint8_t * LinkUpPropertyLabel::getOptions(uint8_t* pSize) {
	*pSize = 3;
	uint8_t * pData = (uint8_t *)calloc(1, *pSize);

	pData[0] = nType;

	memcpy(pData + 1, &nSize, 2);

	return pData;
}

uint8_t * LinkUpPropertyLabel_Int32::getRaw() {
	return ((uint8_t*)&nValue);
}

int32_t LinkUpPropertyLabel_Int32::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_Int32::setValue(int32_t nNewValue) {
	nValue = nNewValue;
}
LinkUpPropertyLabel_Int32::LinkUpPropertyLabel_Int32(const char* pName) {
	init(pName, LinkUpPropertyType::Int32,4);
}