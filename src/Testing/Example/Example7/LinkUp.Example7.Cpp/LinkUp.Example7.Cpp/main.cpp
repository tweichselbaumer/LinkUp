#include <windows.h>
#include <stdio.h>
#include <conio.h>
#include <tchar.h>

#include "LinkUpNode.h"

#define BUFFER_SIZE 1024
#define VALUES 1

int __cdecl main(int argc, char **argv)
{
	HANDLE hPipe = INVALID_HANDLE_VALUE;
	uint8_t pBuffer[BUFFER_SIZE];
	DWORD nBytesRead;
	DWORD nTotalBytesAvail;
	DWORD nBytesLeftThisMessag;
	LinkUpRaw linkUpConnector;
	DWORD nBytesToSend;
	LinkUpNode linkUpNode = {};
	LinkUpLabel* values[VALUES];

	while (true)
	{
		if (hPipe == INVALID_HANDLE_VALUE)
		{
			hPipe = CreateFile(TEXT("\\\\.\\pipe\\linkup"), GENERIC_READ | GENERIC_WRITE, 0, NULL, OPEN_EXISTING, 0, NULL);
			if (hPipe != INVALID_HANDLE_VALUE) {
				linkUpNode.init((char *)"node1");
				for (int i = 0; i < VALUES; i++) {
					char pName[200];
					sprintf(pName, "value%d", i);
					values[i] = linkUpNode.addLabel(pName, LinkUpLabelType::Int32);
				}
			}
			Sleep(1);
		}

		PeekNamedPipe(hPipe, pBuffer, 1024, &nBytesRead, &nTotalBytesAvail, &nBytesLeftThisMessag);
		if (nTotalBytesAvail > 0)
		{
			if (ReadFile(hPipe, pBuffer, 1024, &nBytesRead, NULL))
			{
				if (nBytesRead > 0)
				{
					printf("%d - Receive:\n\t", micros() / 1000);
					for (DWORD i = 0; i < nBytesRead; i++)
					{
						printf("%02X ", pBuffer[i]);
					}
					printf("\n");
					linkUpNode.progress(pBuffer, nBytesRead);
				}
			}
		}
		else
		{
			linkUpNode.progress(NULL, 0);
		}

		nBytesToSend = linkUpNode.getRaw(pBuffer, BUFFER_SIZE);

		if (nBytesToSend > 0)
		{
			printf("%d - Sent:\n\t", micros() / 1000);
			for (DWORD i = 0; i < nBytesToSend; i++)
			{
				printf("%X ", pBuffer[i]);
			}
			printf("\n");
			WriteFile(hPipe, pBuffer, nBytesToSend, 0, 0);
		}
	}

	return 0;
}