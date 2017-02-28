#include "LinkUpNode.h"

#define BUFFER_SIZE 1024

#define DebugStream Serial
#define DataStream Serial1

#define DebugBaud 115200
#define DataBaud 250000

#define PinLed 13

uint8_t nLedStatus = 1;

uint8_t pBuffer[BUFFER_SIZE];
uint32_t nLastTicks = 0;
LinkUpNode linkUpNode;
LinkUpLabel* value1;
LinkUpLabel* value2;

void setup()
{
	DebugStream.begin(DebugBaud);
	DebugStream.setTimeout(1);
	DataStream.begin(DataBaud);
	DataStream.setTimeout(1);

	pinMode(PinLed, OUTPUT);

	linkUpNode.init((char *)"arduino");
	value1 = linkUpNode.addLabel("value1", LinkUpLabelType::Int32);
	value2 = linkUpNode.addLabel("value2", LinkUpLabelType::Int32);
}

void loop()
{
	uint32_t nBytesRead;
	uint32_t nBytesToSend;
	uint32_t nTime = micros();

	if (nTime - nLastTicks > 1000 * 1000 * 0.5)
	{
		nLastTicks = nTime;
		nLedStatus = !nLedStatus;
		digitalWrite(PinLed, nLedStatus);
		value1->set(&nTime);
		uint32_t* v = (uint32_t*)value2->get();
		*v = *v + 1;
	}

	if (DataStream.available())
	{
		nBytesRead = DataStream.readBytes((char*)pBuffer, BUFFER_SIZE);

		if (nBytesRead > 0)
		{
			linkUpNode.progress(pBuffer, nBytesRead);
		}
	}
	else {
		linkUpNode.progress(pBuffer, 0);
	}

	nBytesToSend = linkUpNode.getRaw(pBuffer, BUFFER_SIZE);
	if (nBytesToSend > 0) {
		DataStream.write(pBuffer, nBytesToSend);
	}
}