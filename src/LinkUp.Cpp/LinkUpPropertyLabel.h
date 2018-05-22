#ifndef _LINKUP_PROPERTY_LABEL_h
#define _LINKUP_PROPERTY_LABEL_h

#include "Platform.h"

#include "LinkUpRaw.h"
#include "LinkUpLogic.h"
#include "LinkUpLabel.h"

class LinkUpPropertyLabel :public LinkUpLabel
{
private:
	uint8_t * getOptions(uint8_t* pSize);
	uint16_t nSize;
	LinkUpPropertyType nType;
protected:
	virtual uint8_t * getRaw() = 0;
	void init(const char* pName, LinkUpPropertyType nType, uint16_t nSize);
public:
	bool receivedPropertyGetRequest(uint16_t nIdentifier, LinkUpRaw* pConnector);
	bool receivedPropertySetRequest(uint16_t nIdentifier, uint8_t* pValue, LinkUpRaw* pConnector);
};

class LinkUpPropertyLabel_Int32 :public LinkUpPropertyLabel
{
private:
	int32_t nValue;
protected:
	uint8_t * getRaw();
public:
	int32_t getValue();
	void setValue(int32_t nNewValue);
	LinkUpPropertyLabel_Int32(const char* pName);
};

#endif