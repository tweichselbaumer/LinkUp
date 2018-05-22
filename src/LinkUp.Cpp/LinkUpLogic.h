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
	Property = 2,
	Event = 3,
	Function = 4,
};

enum LinkUpPropertyType : uint8_t
{
	Boolean = 1,
	SByte = 2,
	Byte = 3,
	Int16 = 4,
	UInt16 = 5,
	Int32 = 6,
	UInt32 = 7,
	Int64 = 8,
	UInt64 = 9,
	Single = 10,
	Double = 11,
	Binary = 12
};

PACK(
	struct LinkUpLogic
{
	LinkUpLogicType nLogicType;
	uint8_t pInnerHeader[];
});

PACK(
	struct LinkUpNameRequest
{
	LinkUpLabelType nLabelType;
	uint16_t nNameLength;
	char pName[];
});

PACK(
	struct LinkUpNameResponse
{
	LinkUpLabelType nLabelType;
	uint16_t nIdentifier;
	uint16_t nNameLength;
	char pName[];
});

PACK(
	struct LinkUpPropertyGetRequest
{
	uint16_t nIdentifier;
});

PACK(
	struct LinkUpPropertyGetResponse
{
	uint16_t nIdentifier;
	uint8_t pData[];
});

PACK(
	struct LinkUpPropertySetRequest
{
	uint16_t nIdentifier;
	uint8_t pData[];
});

PACK(
	struct LinkUpPropertySetResponse
{
	uint16_t nIdentifier;
});

#endif