#ifndef _LINKUP_NODE_h
#define _LINKUP_NODE_h

#include "LinkUpRaw.h"

#define INITIALIZATION_TIMEOUT 1000 * 1000 * 1
#define PACKED  __attribute__((packed))

enum LinkUpLogicType : uint8_t
{
	NameRequest = 1,
	NameResponse = 2,
	PropertyGetRequest = 3,
	PropertyGetResponse = 4,
	PropertySetRequest = 5,
	PropertySetResponse = 6
};

enum LinkUpLabelType : uint8_t
{
	Node = 1,
	Event = 2,
	Function = 3,
	Boolean = 4,
	SByte = 5,
	Byte = 6,
	Int16 = 7,
	UInt16 = 8,
	Int32 = 9,
	UInt32 = 10,
	Int64 = 11,
	UInt64 = 12,
	Single = 13,
	Double = 14
};

struct PACKED LinkUpLogic
{
	LinkUpLogicType nLogicType;
	byte pInnerHeader[];
};

struct PACKED LinkUpNameRequest
{
	LinkUpLabelType nLabelType;
	char pName[];
};

struct PACKED LinkUpNameResponse
{
	LinkUpLabelType nLabelType;
	uint16_t nIdentifier;
	char pName[];
};

struct PACKED LinkUpPropertyGetRequest
{
	uint16_t nIdentifier;
};

struct PACKED LinkUpPropertyGetResponse
{
	uint16_t nIdentifier;
	uint8_t pData[];
};

struct PACKED LinkUpPropertySetRequest
{
	uint16_t nIdentifier;
	uint8_t pData[];
};

struct PACKED LinkUpPropertySetResponse
{
	uint16_t nIdentifier;
};

class LinkUpLabel
{
private:
	char* pName;
	uint8_t nSize;
	void* pValue;
	bool isInitialized;
	uint16_t nIdentifier;
	LinkUpLabelType type;
	struct {
		uint32_t nLastInitTry;
	} timestamps;
public:
	void init(char* pName, LinkUpLabelType type);
	void set(void* pValue);
	void* get();
	void progress(LinkUpRaw* pConnector);
	bool completeInitialization(char* pName, LinkUpLabelType type, uint16_t nIdentifier);
	bool getRequest(uint16_t nIdentifier, LinkUpRaw* pConnector);
};

struct LinkUpLabelList
{
	LinkUpLabel *pLabel;
	LinkUpLabelList *pNext;
};

class LinkUpNode
{
private:
	bool isInitialized = false;
	uint16_t nIdentifier = 0;
	LinkUpRaw connector;
	struct {
		uint32_t nLastInitTry;
	} timestamps;
	char* pName = 0;
	LinkUpLabelList* pHead = 0;
	void receivedPacket(LinkUpPacket packet);
	void receivedNameRequest(LinkUpPacket packet, LinkUpNameRequest* pNameRequest);
	void receivedNameResponse(LinkUpPacket packet, LinkUpNameResponse* pNameResponse);
	void receivedPropertyGetRequest(LinkUpPacket packet, LinkUpPropertyGetRequest* pPropertyGetRequest);
	void receivedPropertyGetResponse(LinkUpPacket packet, LinkUpPropertyGetResponse* pPropertyGetResponse);
	void receivedPropertySetRequestt(LinkUpPacket packet, LinkUpPropertySetRequest* pPropertySetRequest);
	void receivedPropertySetResponse(LinkUpPacket packet, LinkUpPropertySetResponse* pPropertySetResponse);
public:
	void progress(uint8_t* pData, uint16_t nCount);
	uint16_t getRaw(uint8_t* pData, uint16_t nMax);
	void init(char* pName);
	LinkUpLabel* addLabel(char* pName, LinkUpLabelType type);
};

#endif