#ifndef _LINKUP_LOGIC_h
#define _LINKUP_LOGIC_h

#include "Platform.h"

enum LinkUpLogicType : uint8_t
{
	NameRequest = 1,
	NameResponse = 2,
	PropertyGetRequest = 3,
	PropertyGetResponse = 4,
	PropertySetRequest = 5,
	PropertySetResponse = 6,
	PingRequest = 7,
	PingResponse = 8
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
	Double = 14,
	Binary = 15
};

struct PACKED LinkUpLogic
{
	LinkUpLogicType nLogicType;
	uint8_t pInnerHeader[];
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

#endif