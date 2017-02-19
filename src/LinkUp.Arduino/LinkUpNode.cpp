#include "LinkUpNode.h"

uint16_t LinkUpNode::getRaw(uint8_t* pData, uint16_t nMax)
{
	return connector.getRaw(pData, nMax);
}

void LinkUpNode::progress(uint8_t* pData, uint16_t nCount)
{
	uint32_t nTime = micros();

	connector.progress(pData, nCount);

	if (connector.hasNext()) {
		receivedPacket(connector.next());
	}

	if (!isInitialized && nTime - timestamps.nLastInitTry > INITIALIZATION_TIMEOUT && pName != NULL) {
		timestamps.nLastInitTry = nTime;

		LinkUpPacket packet;
		packet.nLength = strlen(pName) + sizeof(LinkUpLogic) + sizeof(LinkUpNameRequest);
		packet.pData = (uint8_t*)calloc(packet.nLength, sizeof(uint8_t));

		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		LinkUpNameRequest* nameRequest = (LinkUpNameRequest*)logic->pInnerHeader;

		logic->nLogicType = LinkUpLogicType::NameRequest;
		nameRequest->nLabelType = LinkUpLabelType::Node;
		memcpy(nameRequest->pName, pName, strlen(pName));

		connector.send(packet);
		timestamps.nLastInitTry = nTime;
	}
}

void LinkUpNode::receivedPacket(LinkUpPacket packet)
{
	if (packet.nLength > 0 && packet.pData != NULL) {
		LinkUpLogic* logic = (LinkUpLogic*)packet.pData;
		switch (logic->nLogicType)
		{
		case LinkUpLogicType::NameRequest:
			receivedNameRequest(packet, (LinkUpNameRequest*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::NameResponse:
			receivedNameResponse(packet, (LinkUpNameResponse*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PropertyGetRequest:
			receivedPropertyGetRequest(packet, (LinkUpPropertyGetRequest*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PropertyGetResponse:
			receivedPropertyGetResponse(packet, (LinkUpPropertyGetResponse*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PropertySetRequest:
			receivedPropertySetRequestt(packet, (LinkUpPropertySetRequest*)logic->pInnerHeader);
			break;
		case LinkUpLogicType::PropertySetResponse:
			receivedPropertySetResponse(packet, (LinkUpPropertySetResponse*)logic->pInnerHeader);
			break;
		default:
			break;
		}
	}

	if (packet.pData != NULL) {
		free(packet.pData);
	}
}

void LinkUpNode::receivedNameRequest(LinkUpPacket packet, LinkUpNameRequest* pNameRequest) {
}

void LinkUpNode::receivedNameResponse(LinkUpPacket packet, LinkUpNameResponse* pNameResponse) {
	char *pResponseName;
	uint32_t nLength = packet.nLength - sizeof(LinkUpLogic) - sizeof(LinkUpNameResponse);
	
	pResponseName = (char*)calloc(nLength + 1, sizeof(uint8_t));
	memcpy(pResponseName, pNameResponse->pName, nLength);

	if (pNameResponse->nLabelType == LinkUpLabelType::Node && strcmp(pName, pResponseName) == 0)
	{
		nIdentifier = pNameResponse->nIdentifier;
		isInitialized = true;
	}
}

void LinkUpNode::receivedPropertyGetRequest(LinkUpPacket packet, LinkUpPropertyGetRequest* pPropertyGetRequest) {
}

void LinkUpNode::receivedPropertyGetResponse(LinkUpPacket packet, LinkUpPropertyGetResponse* pPropertyGetResponse) {
}

void LinkUpNode::receivedPropertySetRequestt(LinkUpPacket packet, LinkUpPropertySetRequest* pPropertySetRequest) {
}

void LinkUpNode::receivedPropertySetResponse(LinkUpPacket packet, LinkUpPropertySetResponse* pPropertySetResponse) {
}