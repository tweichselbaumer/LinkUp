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
	PingResponse = 8,
	EventFireRequest = 9,
	EventFireResponse = 10,
	EventSubscribeRequest = 11,
	EventSubscribeResponse = 12,
	EventUnsubscribeRequest = 13,
	EventUnsubscribeResponse = 14,
	FunctionCallRequest = 15,
	FunctionCallResponse = 16
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
	Int8 = 2,
	UInt8 = 3,
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

PACK(
	struct LinkUpEventFireRequest
{
	uint16_t nIdentifier;
	uint8_t pData[];
});

PACK(
	struct LinkUpEventFireResponse
{
	uint16_t nIdentifier;
});

PACK(
	struct LinkUpEventSubscribeRequest
{
	uint16_t nIdentifier;
});

PACK(
	struct LinkUpEventSubscribeResponse
{
	uint16_t nIdentifier;
});

PACK(
	struct LinkUpEventUnsubscribeRequest
{
	uint16_t nIdentifier;
});

PACK(
	struct LinkUpEventUnsubscribeResponse
{
	uint16_t nIdentifier;
});

PACK(
	struct LinkUpFunctionCallRequest
{
	uint16_t nIdentifier;
	uint8_t pData[];
});

PACK(
	struct LinkUpFunctionCallResponse
{
	uint16_t nIdentifier;
	uint8_t pData[];
});

#endif