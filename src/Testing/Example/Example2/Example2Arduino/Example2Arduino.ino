#include "LinkUpRaw.h"

#define BUFFER_SIZE 1024

#define DebugStream Serial
#define DataStream Serial1

#define DebugBaud 250000
#define DataBaud 250000

#define PinLed 13

uint8_t nLedStatus = 1;

uint8_t pBuffer[BUFFER_SIZE];
uint32_t nLastTicks = 0;
LinkUpRaw linkUpConnector;

void setup()
{
	DebugStream.begin(DebugBaud);
	DebugStream.setTimeout(1);
	DataStream.begin(DataBaud);
	DataStream.setTimeout(1);

	pinMode(PinLed, OUTPUT);
}

void loop()
{
	uint32_t nBytesRead;
	uint32_t nBytesToSend;
	uint32_t nTime = micros();

	if (nTime - nLastTicks > 1000 * 1000 * 1)
	{
		nLastTicks = nTime;
		nLedStatus = !nLedStatus;
		digitalWrite(PinLed, nLedStatus);
	}

	if (DataStream.available())
	{
		nBytesRead = DataStream.readBytes((char*)pBuffer, BUFFER_SIZE);

		if (nBytesRead > 0)
		{
			linkUpConnector.progress(pBuffer, nBytesRead);
		}
	}

	if (linkUpConnector.hasNext())
	{
		LinkUpPacket packet = linkUpConnector.next();
		linkUpConnector.send(packet);
		DebugStream.println("*****");
		DebugStream.println("RECEIVED PACKET");
		DebugStream.print("LENGTH: ");
		DebugStream.println(packet.nLength, DEC);
		DebugStream.print("CRC16: 0x");
		DebugStream.println(packet.nCrc, HEX);
		DebugStream.print("DATA: ");
		for (uint16_t i = 0; i < packet.nLength;i++)
		{
			if (packet.pData[i] > 0x0F)
			{
				DebugStream.print("0x");
			}
			else
			{
				DebugStream.print("0x0");
			}
			DebugStream.print(packet.pData[i], HEX);
			DebugStream.print(" ");
		}
		DebugStream.println("");
		DebugStream.println("*****");
	}

	nBytesToSend = linkUpConnector.getRaw(pBuffer, BUFFER_SIZE);

	if (nBytesToSend > 0) {
		DataStream.write(pBuffer, nBytesToSend);
	}
}