#ifndef _LINKUP_LABEL_h
#define _LINKUP_LABEL_h

#include "Platform.h"

#include "LinkUpRaw.h"
#include "LinkUpLogic.h"

class LinkUpLabel
{
private:
	enum {
		initialization_timeout = 1000 * 1000 * 1000
	};
	char* pName;
	uint8_t nSize;
	void* pValue;
	
	LinkUpLabelType type;

	void lock();
	void unlock();

#ifdef LINKUP_BOOST_THREADSAFE
	boost::mutex mtx;
#endif

public:
	struct {
		uint32_t nInitTryTimeout = 0;
	} timestamps;
	bool isInitialized;
	uint16_t nIdentifier;
	void init(const char* pName, LinkUpLabelType type);
	void set(void* pValue);
	void* get();
	void progress(LinkUpRaw* pConnector);
	bool receivedNameResponse(const char* pName, LinkUpLabelType type, uint16_t nIdentifier);
	bool receivedPropertyGetRequest(uint16_t nIdentifier, LinkUpRaw* pConnector);
	bool receivedPropertySetRequest(uint16_t nIdentifier, uint8_t* value, LinkUpRaw* pConnector);
	char* getName();
};

#endif