#ifndef _LINKUP_FUNCTION_LABEL_h
#define _LINKUP_FUNCTION_LABEL_h

#include "Platform.h"
#include "LinkedList.h"
#include "LinkUpRaw.h"
#include "LinkUpLogic.h"
#include "LinkUpLabel.h"
#include "LinkUpNode.h"

class LinkUpNode;

struct LinkUpFunctionData {
	uint32_t nSize;
	uint8_t pData[];
};

typedef uint8_t* (*LinkUpFunctionCalled) (uint8_t* pDataIn, uint32_t nSizeIn, uint32_t* pSizeOut);

class LinkUpFunctionLabel :public LinkUpLabel
{
private:
	uint8_t * getOptions(uint8_t* pSize);
	LinkUpFunctionCalled pFunction;
	LinkedList* pList = new LinkedList();
protected:
	void progressAdv(LinkUpRaw* pConnector);
public:
	void receivedFunctionCallRequest(uint8_t* pData, uint32_t nSize, LinkUpRaw* pConnector);
	LinkUpFunctionLabel(const char* pName, LinkUpNode* pParent, LinkUpFunctionCalled pFunction);
};

#endif