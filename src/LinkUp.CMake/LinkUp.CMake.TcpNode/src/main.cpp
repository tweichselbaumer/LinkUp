#include <cstdlib>
#include <iostream>
#include <memory>
#include <utility>

#include <boost/asio.hpp>
#include <boost/thread.hpp>
#include "tcpserver/TcpServer.h"

using boost::asio::ip::tcp;
using namespace std;

boost::asio::io_service io_service;

void WorkerThread()
{
	std::cout << "Thread Start\n";
	io_service.run();
	std::cout << "Thread Finish\n";
}


int main(int argc, char* argv[])
{
	try
	{
		LinkUpNode linkUpNode = {};
		linkUpNode.init("test");
		boost::shared_ptr< boost::asio::io_service::work > work(
			new boost::asio::io_service::work(io_service)
		);

		TcpServer server(io_service, 3000, &linkUpNode);

		std::cout << "Press [return] to exit." << std::endl;

		boost::thread_group worker_threads;
		worker_threads.create_thread(WorkerThread);

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