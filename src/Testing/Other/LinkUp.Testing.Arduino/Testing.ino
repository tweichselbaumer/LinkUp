#include "LinkUpRaw.h"

#define BUFFER_SIZE 1024

#define DebugStream Serial
#define DataStream Serial1

uint8_t pBuffer[BUFFER_SIZE];
uint32_t nLastTicks = 0;
LinkUpRaw linkUpConnector;

void setup()
{
	DebugStream.begin(1);
	DebugStream.setTimeout(1);
	DataStream.begin(250000);
	DataStream.setTimeout(1);
}

void loop_testLogic()
{
}

void loop_testRaw()
{
	uint32_t nBytesRead;
	uint32_t nBytesToSend;
	uint32_t nTime = micros();

	if (nTime - nLastTicks > 1000 * 1000 * 1)
	{
		nLastTicks = nTime;
		DebugStream.println("Running");
	}

	if (DataStream.available())
	{
		nBytesRead = DataStream.readBytes((char*)pBuffer, BUFFER_SIZE);
		DebugStream.println("ToRead:");
		DebugStream.println(nBytesRead);
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
		DebugStream.print("LENGHT: ");
		DebugStream.println(packet.nLenght, DEC);
		DebugStream.print("CRC16: 0x");
		DebugStream.println(packet.nCrc, HEX);
		DebugStream.print("DATA: ");
		for (uint16_t i = 0; i < packet.nLenght;i++)
		{
			DebugStream.print("0x");
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

void loop() {
	loop_testRaw();
	//loop_testLogic();
}