#include "TcpServer.h"
#include "Session.h"

#include <cstdlib>
#include <iostream>

using boost::asio::ip::tcp;
using namespace std;


TcpServer::TcpServer(boost::asio::io_service& io_service, short port, LinkUpNode* node, uint8_t maxConnections)
	: acceptor_(io_service, tcp::endpoint(tcp::v4(), port)),
	socket_(io_service)
{
	connections_ = 0;
	maxConnections_ = maxConnections;
	node_ = node;
	do_accept();
}

void TcpServer::removeSession(Session* pSession)
{
	if (connections_ != 0)
		std::cout << "Closed connection [" << pSession->address_.to_string() << ":" << pSession->port_ << "]" << std::endl;
	connections_ = 0;
	do_accept();
}

void TcpServer::do_accept()
{
	acceptor_.async_accept(socket_,
		[this](boost::system::error_code ec)
	{
		if (!ec)
		{
			if (connections_ == 0)
			{
				std::cout << "Create new connection [" << socket_.remote_endpoint().address().to_string() << ":" << socket_.remote_endpoint().port() << "]" << std::endl;
				connections_ = 1;
				do_accept();
				std::make_shared<Session>(std::move(socket_), this, node_)->start();
			}
			else
			{
				std::cout << "Drop connection [" << socket_.remote_endpoint().address().to_string() << ":" << socket_.remote_endpoint().port() << "]" << std::endl;
				socket_.close();
				do_accept();
			}
		}
	});
}


