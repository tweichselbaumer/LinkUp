#include "LinkUp.h"

#define BUFFER_SIZE 1024

uint8_t pBuffer[BUFFER_SIZE];
uint8_t nBytesRead;
uint32_t nLastTicks;
uint32_t i;

bool flip = false;

void setup()
{
	Serial.begin(1);
	Serial.setTimeout(1);
	Serial.println("START");
	Serial.flush();
	Serial1.begin(3000000); 
	Serial1.setTimeout(1);
}

void loop()
{ 
	uint32_t time = micros();
	if (time-nLastTicks > 1 * 1000 * 1000) 
	{
		Serial1.print("\n Performance [KB/s]: ");
		Serial1.print((1000*1000*i*((double)BUFFER_SIZE) / (1024 * (time - nLastTicks))));
		Serial1.print("\n Count: ");
		Serial1.print(i);
		Serial1.flush();
		nLastTicks = time;
		i = 0;
	}


	Serial.write(pBuffer, BUFFER_SIZE);

	
	i++;
	/*
	uint32_t time;
	time = micros();

	if ((time - nLastTicks) > (uint32_t)1000 * 1000 * 2)
	{
		LinkUpPacket packet = {0};
		uint8_t pData[100];

		packet.nLenght = 5;
		packet.pData = (uint8_t*)calloc(5, sizeof(uint8_t));
		packet.pData[0] = LINKUP_PREAMBLE;
		packet.pData[1] = LINKUP_EOP;
		packet.pData[2] = LINKUP_SKIP;
		packet.pData[3] = 1;
		packet.pData[4] = 2;

		LinkUp.send(packet);
		uint8_t size = LinkUp.getRaw(pData, 100);

		Serial.print("\nSEND PACKET BEFORE: ");
		Serial1.print("\nSEND PACKET BEFORE: ");

		for (int i = 0; i < 5; i++) 
		{
			Serial.print("0x");
			Serial.print(packet.pData[i], HEX);
			Serial.print(" ");
		}
		Serial.print("\nSEND PACKET AFTER: ");

		for (int i = 0; i < size; i++)
		{
			Serial.print("0x");
			Serial.print(pData[i], HEX);
			Serial.print(" ");
		}

		flip = !flip;

		if (flip)
		{
			Serial.print("\nNO ERROR");
			LinkUp.progress(pData, size);
		}
		else
		{
			Serial.print("\nERROR");
			LinkUp.progress(pData, size - 2);
		}
	
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
		Serial.print(packet.nLenght, DEC);
		Serial.print("\nCRC16: 0x");
		Serial.print(packet.nCrc, HEX);
		Serial.print("\nDATA: ");
		for (uint8_t i = 0; i < packet.nLenght;i++)
		{
			Serial.print("0x");
			Serial.print(packet.pData[i], HEX);
			Serial.print(" ");
		}
		Serial.print("\n*****\n\n");
		free(packet.pData);
	}
	Serial.flush();
	*/
}