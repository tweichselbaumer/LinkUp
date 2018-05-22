#include <cstdlib>
#include <iostream>
#include <memory>
#include <utility>

#include <boost/asio.hpp>
#include <boost/thread.hpp>
#include "socket/tcp_server.h"

#include "AVLTree.h"

using boost::asio::ip::tcp;
using namespace std;

boost::asio::io_service io_service;

void doWork()
{
	io_service.run();
}


int main(int argc, char* argv[])
{
	try
	{
		LinkUpNode* pLinkUpNode = new LinkUpNode("test");

		for (int i = 1; i <= 100; i++) {
			char str[25];
			sprintf(str, "label%d", i);
			LinkUpPropertyLabel_Int32* pLabel = new  LinkUpPropertyLabel_Int32(str);
			pLabel->setValue(12);
			pLinkUpNode->addLabel(pLabel);
		}
		boost::shared_ptr< boost::asio::io_service::work > work(
			new boost::asio::io_service::work(io_service)
		);

		tcp_server server(io_service, 3000, pLinkUpNode, 1);

		std::cout << "Press [return] to exit." << std::endl;

		boost::thread_group worker_threads;
		worker_threads.create_thread(doWork);

		std::cin.get();

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