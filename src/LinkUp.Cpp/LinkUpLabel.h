#ifndef _LINKUP_LABEL_h
#define _LINKUP_LABEL_h

#include "Platform.h"
#include "LinkUpRaw.h"
#include "LinkUpLogic.h"

class LinkUpNode;

class LinkUpLabel
{
private:
	enum {
		initialization_timeout = 1000 * 1000 * 4
	};

#ifdef LINKUP_BOOST_THREADSAFE
	boost::mutex mtx;
#endif

protected:
	void lock();
	void unlock();
	LinkUpNode * pParent;
	virtual uint8_t* getOptions(uint8_t* pSize) = 0;
	void init(const char* pName, LinkUpLabelType type, LinkUpNode* pParent);
	virtual void progressAdv(LinkUpRaw* pConnector) = 0;
public:
	void progress(LinkUpRaw* pConnector);
	struct {
		uint32_t nInitTryTimeout = 0;
	} timestamps;
	LinkUpLabelType nType;
	char* pName = 0;
	bool isInitialized = false;
	uint16_t nIdentifier = 0;

	bool receivedNameResponse(const char* pName, LinkUpLabelType type, uint16_t nIdentifier);
};

#include "LinkUpNode.h"

#endif