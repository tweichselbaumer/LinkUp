#ifndef _LINKUP_EVENT_LABEL_h
#define _LINKUP_EVENT_LABEL_h

#include "Platform.h"
#include "LinkedList.h"
#include "LinkUpRaw.h"
#include "LinkUpLogic.h"
#include "LinkUpLabel.h"
#include "LinkUpNode.h"

struct LinkUpEventData {
	uint16_t nSize;
	uint8_t pData[];
};

class LinkUpNode;

class LinkUpEventLabel :public LinkUpLabel
{
private:
	uint8_t * getOptions(uint8_t* pSize);
	LinkedList* pList = new LinkedList();
	bool isSubscribed;
protected:
	void progressAdv(LinkUpRaw* pConnector);
public:
	void subscribed(LinkUpRaw* pConnector);
	void unsubscribed(LinkUpRaw* pConnector);
	void fireEvent(uint8_t * pData, uint16_t nSize);
	LinkUpEventLabel(const char* pName, LinkUpNode* pParent);
};

#endif