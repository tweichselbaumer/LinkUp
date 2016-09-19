#include "LinkUp.h"

#define BUFFER_SIZE 32

uint8_t pBuffer[BUFFER_SIZE];
uint8_t nBytesRead;
uint32_t nLastTicks;

void setup()
{
	Serial.begin(115200);
	Serial.setTimeout(1);
	Serial.println("START");
	Serial.flush();
}

void loop()
{
	uint32_t time;
	time = micros();

	if ((time - nLastTicks) > (uint32_t)1000 * 1000 * 2)
	{
		nLastTicks = time;
		Serial.print("\nRECEIVED PACKET: ");
		Serial.print(LinkUp.nTotalReceivedPackets, DEC);
		Serial.print("\nFAILED PACKET: ");
		Serial.print(LinkUp.nTotalFailedPackets, DEC);
		Serial.print("\n");
	}

	if (Serial.available())
	{
		nBytesRead = Serial.readBytes((char*)pBuffer, BUFFER_SIZE);
		if (nBytesRead > 0)
		{
			LinkUp.progress(pBuffer, nBytesRead);
		}
	}
	if (LinkUp.hasNext()) 
	{
		LinkUpPacket packet = LinkUp.next();
		Serial.print("*****\nRECEIVED PACKET");
		Serial.print("\nLENGHT: ");
		Serial.print(packet.lenght, DEC);
		Serial.print("\nCRC16: 0x");
		Serial.print(packet.crc, HEX);
		Serial.print("\nDATA: ");
		for (uint8_t i = 0; i < packet.lenght;i++)
		{
			Serial.print("0x");
			Serial.print(packet.data[i], HEX);
			Serial.print(" ");
		}
		Serial.print("\n*****\n\n");
		free(packet.data);
	}
	Serial.flush();
}