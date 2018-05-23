#include "LinkUpLabel.h"

void LinkUpLabel::progress(LinkUpRaw* pConnector) {
	uint32_t nTime = getSystemTime();

	if (!isInitialized && nTime > timestamps.nInitTryTimeout && pName != NULL) {
		timestamps.nInitTryTimeout = nTime + initialization_timeout;

		uint8_t nOptionSize = 0;
		uint8_t* pOptions = getOptions(&nOptionSize);

		LinkUpPacket packet;
		packet.nLength = strlen(pName) + (uint16_t)sizeof(LinkUpLogic) + (uint16_t)sizeof(LinkUpNameRequest) + nOptionSize;
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		LinkUpNameRequest* nameRequest = (LinkUpNameRequest*)logic->pInnerHeader;

		logic->nLogicType = LinkUpLogicType::NameRequest;
		nameRequest->nLabelType = nType;
		nameRequest->nNameLength = strlen(pName);
		
		memcpy(nameRequest->pName, pName, nameRequest->nNameLength);

		if (nOptionSize > 0) {
			memcpy(nameRequest->pName + nameRequest->nNameLength, pOptions, nOptionSize);
			free(pOptions);
		}
		pConnector->send(packet);
	}
	progressAdv(pConnector);
}

void LinkUpLabel::init(const char* pName, LinkUpLabelType type, LinkUpNode* pParent)
{
	this->pParent = pParent;
	this->pName = (char*)calloc(strlen(pName) + 1, sizeof(uint8_t));
	strcpy(this->pName, pName);
	this->nType = type;
	pParent->addLabel(this);
}

bool LinkUpLabel::receivedNameResponse(const char* pName, LinkUpLabelType type, uint16_t nIdentifier) {
	if (strcmp(this->pName, pName) == 0 && this->nType == type)
	{
		isInitialized = true;
		this->nIdentifier = nIdentifier;
		return true;
	}
	else
	{
		return false;
	}
}