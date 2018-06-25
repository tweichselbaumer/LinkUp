#include <cstdlib>
#include <iostream>
#include <memory>
#include <utility>

#include <boost/asio.hpp>
#include <boost/timer/timer.hpp>
#include <boost/thread.hpp>
#include "socket/tcp_server.h"

#include "AvlTree.h"
#include "Platform.h"

using boost::asio::ip::tcp;

using namespace boost::timer;
using namespace std;

boost::asio::io_service io_service;

LinkUpEventLabel* pEvent;
LinkUpNode* pLinkUpNode;

bool running = true;

void doWork()
{
	io_service.run();
}

void doWork2()
{
	uint8_t* pData = (uint8_t*)calloc(1024 * 1024 * 10, sizeof(uint8_t));
	pData[0] = LINKUP_RAW_SKIP;
	pData[5] = LINKUP_RAW_SKIP;
	pData[6] = LINKUP_RAW_EOP;
	pData[7] = LINKUP_RAW_SKIP;
	pData[8] = LINKUP_RAW_EOP;
	pData[12] = LINKUP_RAW_SKIP;
	pData[13] = LINKUP_RAW_EOP;
	pData[20] = LINKUP_RAW_SKIP;
	pData[34] = LINKUP_RAW_EOP;

	while (running) {
		pEvent->fireEvent((uint8_t*)pData, 512);
		boost::this_thread::sleep_for(boost::chrono::milliseconds(1000 / 50));
	}
}

void doWork3()
{
	while (running) {
		pLinkUpNode->progress(0, 0, 1000, false);
		boost::this_thread::sleep_for(boost::chrono::milliseconds(1));
	}
}

uint8_t * myFunc(uint8_t* pDataIn, uint32_t nSizeIn, uint32_t* pSizeOut)
{
	*pSizeOut = nSizeIn;
	uint8_t* pDataOut = (uint8_t*)calloc(nSizeIn, sizeof(uint8_t));
	memcpy(pDataOut, pDataIn, *pSizeOut);
	return pDataOut;
}

int main(int argc, char* argv[])
{
	try
	{
		pLinkUpNode = new LinkUpNode("test");

		/*for (int i = 1; i <= 5; i++) {
			char str[25] = { 0 };
			sprintf(str, "label_int_%d", i);
			LinkUpPropertyLabel_Int32* pLabel = new  LinkUpPropertyLabel_Int32(str, pLinkUpNode);
			pLabel->setValue(12);
		}

		for (int i = 1; i <= 5; i++) {
			char str[25] = { 0 };
			sprintf(str, "label_bin_%d", i);
			LinkUpPropertyLabel_Binary* pLabel = new  LinkUpPropertyLabel_Binary(str, 25, pLinkUpNode);
			pLabel->setValue((uint8_t*)str);
		}*/

		pEvent = new  LinkUpEventLabel("label_event", pLinkUpNode);
		new LinkUpFunctionLabel("label_function", pLinkUpNode, &myFunc);

		boost::shared_ptr< boost::asio::io_service::work > work(
			new boost::asio::io_service::work(io_service)
		);

		tcp_server server(io_service, 3000, pLinkUpNode, 1);

		std::cout << "Press [return] to exit." << std::endl;

		boost::thread_group worker_threads;
		worker_threads.create_thread(doWork);
		worker_threads.create_thread(doWork2);
		worker_threads.create_thread(doWork3);

		std::cin.get();

		running = false;
		io_service.stop();

		worker_threads.join_all();

		return 0;
	}
	catch (std::exception& e)
	{
		std::cerr << "Exception: " << e.what() << "\n";
	}

	return 0;
}