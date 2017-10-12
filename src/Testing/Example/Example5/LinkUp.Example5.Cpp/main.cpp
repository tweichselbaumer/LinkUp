#include <windows.h> 
#include <stdio.h>
#include <conio.h>
#include <tchar.h>

#include "LinkUpNode.h"

#define BUFFER_SIZE 1024
#define VALUES 2

int __cdecl main(int argc, char **argv)
{
	HANDLE hPipe = INVALID_HANDLE_VALUE;
	uint8_t pBuffer[BUFFER_SIZE];
	DWORD nBytesRead;
	DWORD nTotalBytesAvail;
	DWORD nBytesLeftThisMessag;
	LinkUpRaw linkUpConnector;
	DWORD nBytesToSend;

	while (true)
	{
		if (hPipe == INVALID_HANDLE_VALUE)
		{
			hPipe = CreateFile(TEXT("\\\\.\\pipe\\linkup"), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
		}
		if (!PeekNamedPipe(hPipe, pBuffer, 1024, &nBytesRead, &nTotalBytesAvail, &nBytesLeftThisMessag))
		{
			hPipe = INVALID_HANDLE_VALUE;
			nTotalBytesAvail = 0;
		}
		if (nTotalBytesAvail > 0)
		{
			if (ReadFile(hPipe, pBuffer, 1024, &nBytesRead, NULL))
			{
				if (nBytesRead > 0)
				{
					linkUpConnector.progress(pBuffer, nBytesRead);
				}
			}
		}

		if (linkUpConnector.hasNext())
		{
			LinkUpPacket packet = linkUpConnector.next();
			linkUpConnector.send(packet);
			printf("*****\n");
			printf("RECEIVED PACKET\n");
			printf("LENGTH: %d\n", packet.nLength);
			printf("CRC16: %02X\n", packet.nCrc);
			printf("DATA: ");
			for (uint16_t i = 0; i < packet.nLength;i++)
			{
				printf("%02X ", pBuffer[i]);
			}
			printf("\n*****\n");
		}

		nBytesToSend = linkUpConnector.getRaw(pBuffer, BUFFER_SIZE);

		if (nBytesToSend > 0)
		{
			WriteFile(hPipe, pBuffer, nBytesToSend, 0, 0);
		}
	}

	return 0;
}
