#include "LinkUpFunctionLabel.h"

uint8_t * LinkUpFunctionLabel::getOptions(uint8_t* pSize)
{
	*pSize = 0;
	return NULL;
}

void LinkUpFunctionLabel::progressAdv(LinkUpRaw* pConnector)
{
	lock();
	LinkedListIterator iterator(pList);
	unlock();

	LinkUpFunctionData* pData;

	int i = 0;

	while ((pData = (LinkUpFunctionData*)iterator.next()) != NULL)
	{
		i++;
		lock();
		pList->remove(pData);
		unlock();

		uint32_t nSize;
		uint8_t* pResult = pFunction(pData->pData, pData->nSize, &nSize);

		LinkUpPacket packet;
		packet.nLength = sizeof(LinkUpLogic) + sizeof(LinkUpFunctionCallResponse) + nSize;
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* pLogic = (LinkUpLogic*)packet.pData;
		pLogic->nLogicType = LinkUpLogicType::FunctionCallResponse;
		LinkUpFunctionCallResponse* pFunctionCallResponse = (LinkUpFunctionCallResponse*)pLogic->pInnerHeader;
		pFunctionCallResponse->nIdentifier = nIdentifier;

		if (pResult != NULL && nSize > 0)
		{
			memcpy(pFunctionCallResponse->pData, pResult, nSize);
			free(pResult);
		}

		if (pData->nSize > 1024)
		{
			pConnector->send(packet, false);
		}
		else
		{
			pConnector->send(packet);
		}

		free(pData);
	}

#ifdef LINKUP_DEBUG
	/*if (i > 0)
		cout << "Received " << i << " function calls." << endl;*/
#endif

}

void LinkUpFunctionLabel::receivedFunctionCallRequest(uint8_t* pData, uint32_t nSize, LinkUpRaw* pConnector)
{
	lock();
	if (pFunction != NULL)
	{
		LinkUpFunctionData* pFunctionData = (LinkUpFunctionData*)calloc(1, sizeof(LinkUpFunctionData) + nSize);
		pFunctionData->nSize = nSize;
		if (nSize > 0) {
			memcpy(pFunctionData->pData, pData, nSize);
		}
		pList->insert(pFunctionData);
	}
	unlock();
}

void LinkUpFunctionLabel::setFunction(LinkUpFunctionCalled pFunction)
{
	this->pFunction = pFunction;
}

LinkUpFunctionLabel::LinkUpFunctionLabel(const char* pName, LinkUpNode* pParent)
{
	init(pName, LinkUpLabelType::Function, pParent);
}