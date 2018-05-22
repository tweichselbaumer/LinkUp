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

uint8_t * LinkUpPropertyLabel_Boolean::getRaw() {
	return ((uint8_t*)&nValue);
}

bool LinkUpPropertyLabel_Boolean::getValue() {
	return nValue;
}

void LinkUpPropertyLabel_Boolean::setValue(bool nNewValue) {
	nValue = nNewValue;
}

LinkUpPropertyLabel_Boolean::LinkUpPropertyLabel_Boolean(const char* pName) {
	init(pName, LinkUpPropertyType::Boolean, 1);
}

uint8_t * LinkUpPropertyLabel_Int8::getRaw() {
	return ((uint8_t*)&nValue);
}

int8_t LinkUpPropertyLabel_Int8::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_Int8::setValue(int8_t nNewValue) {
	nValue = nNewValue;
}

LinkUpPropertyLabel_Int8::LinkUpPropertyLabel_Int8(const char* pName) {
	init(pName, LinkUpPropertyType::Int8, 1);
}

uint8_t * LinkUpPropertyLabel_UInt8::getRaw() {
	return ((uint8_t*)&nValue);
}

uint8_t LinkUpPropertyLabel_UInt8::getValue() {
	return nValue;
}

void LinkUpPropertyLabel_UInt8::setValue(uint8_t nNewValue) {
	nValue = nNewValue;
}

LinkUpPropertyLabel_UInt8::LinkUpPropertyLabel_UInt8(const char* pName) {
	init(pName, LinkUpPropertyType::UInt8, 1);
}

uint8_t * LinkUpPropertyLabel_Int16::getRaw() {
	return ((uint8_t*)&nValue);
}

int16_t LinkUpPropertyLabel_Int16::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_Int16::setValue(int16_t nNewValue) {
	nValue = nNewValue;
}
LinkUpPropertyLabel_Int16::LinkUpPropertyLabel_Int16(const char* pName) {
	init(pName, LinkUpPropertyType::Int16, 2);
}

uint8_t * LinkUpPropertyLabel_UInt16::getRaw() {
	return ((uint8_t*)&nValue);
}

uint16_t LinkUpPropertyLabel_UInt16::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_UInt16::setValue(uint16_t nNewValue) {
	nValue = nNewValue;
}
LinkUpPropertyLabel_UInt16::LinkUpPropertyLabel_UInt16(const char* pName) {
	init(pName, LinkUpPropertyType::UInt16, 4);
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
	init(pName, LinkUpPropertyType::Int32, 4);
}

uint8_t * LinkUpPropertyLabel_UInt32::getRaw() {
	return ((uint8_t*)&nValue);
}

uint32_t LinkUpPropertyLabel_UInt32::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_UInt32::setValue(uint32_t nNewValue) {
	nValue = nNewValue;
}
LinkUpPropertyLabel_UInt32::LinkUpPropertyLabel_UInt32(const char* pName) {
	init(pName, LinkUpPropertyType::UInt32, 4);
}

uint8_t * LinkUpPropertyLabel_Int64::getRaw() {
	return ((uint8_t*)&nValue);
}

int64_t LinkUpPropertyLabel_Int64::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_Int64::setValue(int64_t nNewValue) {
	nValue = nNewValue;
}
LinkUpPropertyLabel_Int64::LinkUpPropertyLabel_Int64(const char* pName) {
	init(pName, LinkUpPropertyType::Int64, 8);
}

uint8_t * LinkUpPropertyLabel_UInt64::getRaw() {
	return ((uint8_t*)&nValue);
}

uint64_t LinkUpPropertyLabel_UInt64::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_UInt64::setValue(uint64_t nNewValue) {
	nValue = nNewValue;
}
LinkUpPropertyLabel_UInt64::LinkUpPropertyLabel_UInt64(const char* pName) {
	init(pName, LinkUpPropertyType::UInt64, 8);
}

uint8_t * LinkUpPropertyLabel_Single::getRaw() {
	return ((uint8_t*)&nValue);
}

float_t LinkUpPropertyLabel_Single::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_Single::setValue(float_t nNewValue) {
	nValue = nNewValue;
}
LinkUpPropertyLabel_Single::LinkUpPropertyLabel_Single(const char* pName) {
	init(pName, LinkUpPropertyType::Single, 4);
}

uint8_t * LinkUpPropertyLabel_Double::getRaw() {
	return ((uint8_t*)&nValue);
}

double_t LinkUpPropertyLabel_Double::getValue() {
	return nValue;
}
void LinkUpPropertyLabel_Double::setValue(double_t nNewValue) {
	nValue = nNewValue;
}
LinkUpPropertyLabel_Double::LinkUpPropertyLabel_Double(const char* pName) {
	init(pName, LinkUpPropertyType::Double, 8);
}

uint8_t * LinkUpPropertyLabel_Binary::getRaw() {
	return pValue;
}

uint8_t* LinkUpPropertyLabel_Binary::getValue() {
	uint8_t *pTemp = (uint8_t*)calloc(nSize, sizeof(uint8_t));
	memcpy(pTemp, pValue, nSize);
	return pTemp;
}
void LinkUpPropertyLabel_Binary::setValue(uint8_t* pNewValue) {
	memcpy(pValue, pNewValue, nSize);
}
LinkUpPropertyLabel_Binary::LinkUpPropertyLabel_Binary(const char* pName, uint16_t nSize) {
	pValue = (uint8_t*)calloc(nSize, sizeof(uint8_t));
	init(pName, LinkUpPropertyType::Binary, nSize);
}

LinkUpPropertyLabel_Binary::~LinkUpPropertyLabel_Binary() {
	free(pValue);
}