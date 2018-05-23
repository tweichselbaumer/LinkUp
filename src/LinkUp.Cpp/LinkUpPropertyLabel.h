#ifndef _LINKUP_PROPERTY_LABEL_h
#define _LINKUP_PROPERTY_LABEL_h

#include "Platform.h"

#include "LinkUpRaw.h"
#include "LinkUpLogic.h"
#include "LinkUpLabel.h"
#include "LinkUpNode.h"

class LinkUpLabel;
class LinkUpNode;

class LinkUpPropertyLabel :public LinkUpLabel
{
private:
	uint8_t * getOptions(uint8_t* pSize);
	LinkUpPropertyType nType;
protected:
	uint16_t nSize;
	virtual uint8_t * getRaw() = 0;
	void init(const char* pName, LinkUpPropertyType nType, uint16_t nSize, LinkUpNode* pParent);
	void progressAdv(LinkUpRaw* pConnector);
public:
	bool receivedPropertyGetRequest(uint16_t nIdentifier, LinkUpRaw* pConnector);
	bool receivedPropertySetRequest(uint16_t nIdentifier, uint8_t* pValue, LinkUpRaw* pConnector);
};

class LinkUpPropertyLabel_Boolean :public LinkUpPropertyLabel
{
private:
	bool nValue;
protected:
	uint8_t * getRaw();
public:
	bool getValue();
	void setValue(bool nNewValue);
	LinkUpPropertyLabel_Boolean(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_Int8 :public LinkUpPropertyLabel
{
private:
	int8_t nValue;
protected:
	uint8_t * getRaw();
public:
	int8_t getValue();
	void setValue(int8_t nNewValue);
	LinkUpPropertyLabel_Int8(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_UInt8 :public LinkUpPropertyLabel
{
private:
	uint8_t nValue;
protected:
	uint8_t * getRaw();
public:
	uint8_t getValue();
	void setValue(uint8_t nNewValue);
	LinkUpPropertyLabel_UInt8(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_Int16 :public LinkUpPropertyLabel
{
private:
	int16_t nValue;
protected:
	uint8_t * getRaw();
public:
	int16_t getValue();
	void setValue(int16_t nNewValue);
	LinkUpPropertyLabel_Int16(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_UInt16 :public LinkUpPropertyLabel
{
private:
	uint16_t nValue;
protected:
	uint8_t * getRaw();
public:
	uint16_t getValue();
	void setValue(uint16_t nNewValue);
	LinkUpPropertyLabel_UInt16(const char* pName, LinkUpNode* pParent);
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
	LinkUpPropertyLabel_Int32(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_UInt32 :public LinkUpPropertyLabel
{
private:
	uint32_t nValue;
protected:
	uint8_t * getRaw();
public:
	uint32_t getValue();
	void setValue(uint32_t nNewValue);
	LinkUpPropertyLabel_UInt32(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_Int64 :public LinkUpPropertyLabel
{
private:
	int64_t nValue;
protected:
	uint8_t * getRaw();
public:
	int64_t getValue();
	void setValue(int64_t nNewValue);
	LinkUpPropertyLabel_Int64(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_UInt64 :public LinkUpPropertyLabel
{
private:
	uint64_t nValue;
protected:
	uint8_t * getRaw();
public:
	uint64_t getValue();
	void setValue(uint64_t nNewValue);
	LinkUpPropertyLabel_UInt64(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_Single :public LinkUpPropertyLabel
{
private:
	float_t nValue;
protected:
	uint8_t * getRaw();
public:
	float_t getValue();
	void setValue(float_t nNewValue);
	LinkUpPropertyLabel_Single(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_Double :public LinkUpPropertyLabel
{
private:
	double_t nValue;
protected:
	uint8_t * getRaw();
public:
	double_t getValue();
	void setValue(double_t nNewValue);
	LinkUpPropertyLabel_Double(const char* pName, LinkUpNode* pParent);
};

class LinkUpPropertyLabel_Binary :public LinkUpPropertyLabel
{
private:
	uint8_t * pValue;
protected:
	uint8_t * getRaw();
public:
	uint8_t * getValue();
	void setValue(uint8_t* pNewValue);
	LinkUpPropertyLabel_Binary(const char* pName, uint16_t nSize, LinkUpNode* pParent);
	~LinkUpPropertyLabel_Binary();
};

#endif