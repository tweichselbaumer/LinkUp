#include "LinkUp.h"

#define BUFFER_SIZE 255

#define DebugStream Serial
#define DataStream Serial1

uint8_t pBuffer[BUFFER_SIZE];
uint32_t nLastTicks = 0;


void setup()
{
	DebugStream.begin(1);
	DebugStream.setTimeout(1);
	DataStream.begin(3000000);
	DataStream.setTimeout(1);
}

void loop()
{
	uint32_t nBytesRead;
	uint32_t nBytesToSend;
	uint32_t nTime = micros();

	if (nTime - nLastTicks > 1000 * 1000 * 5) 
	{
		nLastTicks = nTime;
		DebugStream.print("\nRunning\n");
	}

	if (DataStream.available())
	{
		nBytesRead = DataStream.readBytes((char*)pBuffer, BUFFER_SIZE);
		if (nBytesRead > 0)
		{
			LinkUp.progress(pBuffer, nBytesRead);
		}
	}

	if (LinkUp.hasNext())
	{
		LinkUpPacket packet = LinkUp.next();
		LinkUp.send(packet);

		DebugStream.print("*****\nRECEIVED PACKET");
		DebugStream.print("\nLENGHT: ");
		DebugStream.print(packet.nLenght, DEC);
		DebugStream.print("\nCRC16: 0x");
		DebugStream.print(packet.nCrc, HEX);
		DebugStream.print("\nDATA: ");
		for (uint8_t i = 0; i < packet.nLenght;i++)
		{
			DebugStream.print("0x");
			DebugStream.print(packet.pData[i], HEX);
			DebugStream.print(" ");
		}
		DebugStream.print("\n*****\n\n");
	}

	nBytesToSend = LinkUp.getRaw(pBuffer, BUFFER_SIZE);

	if (nBytesToSend > 0) {
		DataStream.write(pBuffer, nBytesToSend);
	}
}