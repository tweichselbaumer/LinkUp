#ifndef _LINKUP_NODE_h
#define _LINKUP_NODE_h

#include "Platform.h"

#include "LinkUpRaw.h"
#include "LinkUpLogic.h"
#include "LinkedList.h"
#include "AVLTree.h"

#include "LinkUpLabel.h"
#include "LinkUpPropertyLabel.h"
#include "LinkUpEventLabel.h"

class LinkUpLabel;

class LinkUpNode
{
private:
	enum {
		initialization_timeout = 1000 * 1000 * 10,
		ping_timeout = 1000 * 1000 * 5
	};
	bool isInitialized = false;
	uint16_t nIdentifier = 0;
	LinkUpRaw connector = {};
	struct {
		uint32_t nInitTryTimeout = 0;
		uint32_t nPingTimeout = 0;
	} timestamps;

	char* pName = 0;

	LinkedList *pList = new LinkedList();
	LinkedList *pEventList = new LinkedList();

	AvlTree *pAvlTree = new AvlTree();
	void receivedPacket(LinkUpPacket packet, uint32_t nTime);
	void receivedNameRequest(LinkUpPacket packet, LinkUpNameRequest* pNameRequest);
	void receivedNameResponse(LinkUpPacket packet, LinkUpNameResponse* pNameResponse);
	void receivedPropertyGetRequest(LinkUpPacket packet, LinkUpPropertyGetRequest* pPropertyGetRequest);
	void receivedPropertyGetResponse(LinkUpPacket packet, LinkUpPropertyGetResponse* pPropertyGetResponse);
	void receivedPropertySetRequest(LinkUpPacket packet, LinkUpPropertySetRequest* pPropertySetRequest);
	void receivedPropertySetResponse(LinkUpPacket packet, LinkUpPropertySetResponse* pPropertySetResponse);
	void receivedPingRequest(LinkUpPacket packet, uint32_t nTime);
	void receivedEventFireResponse(LinkUpPacket packet, LinkUpEventFireResponse* pEventFireResponse);
	void receivedEventSubscribeRequest(LinkUpPacket packet, LinkUpEventSubscribeRequest* pEventSubscribeRequest);
	void receivedEventUnsubscribeRequest(LinkUpPacket packet, LinkUpEventUnsubscribeRequest* pEventUnsubscribeRequest);
	void lock();
	void unlock();

#ifdef LINKUP_BOOST_THREADSAFE
	boost::mutex mtx;
#endif
public:
	void progress(uint8_t* pData, uint16_t nCount, uint16_t nMax);
	uint16_t getRaw(uint8_t* pData, uint16_t nMax);
	LinkUpNode(const char* pName);
	void addLabel(LinkUpLabel* pLabel);
};

#endif