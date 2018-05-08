#include "TcpServer.h"

#include <cstdlib>
#include <iostream>

using boost::asio::ip::tcp;
using namespace std;

class session
	: public std::enable_shared_from_this<session>
{
public:
	session(tcp::socket socket)
		: socket_(std::move(socket))
	{
	}

	void start()
	{
		do_read();
	}

private:
	void do_read()
	{
		auto self(shared_from_this());
		socket_.async_read_some(boost::asio::buffer(data_, max_length),
			[this, self](boost::system::error_code ec, std::size_t length)
		{
			if (socket_.is_open()) {
				std::cout << ec << std::endl;
				if (ec==0)
				{
					do_write(length);
				}
				else {
					std::cout << "Connection down" << std::endl;
				}
			}
		});
	}


	void do_write(std::size_t length)
	{
		auto self(shared_from_this());
		boost::asio::async_write(socket_, boost::asio::buffer(data_, length),
			[this, self](boost::system::error_code ec, std::size_t /*length*/)
		{
			if (!ec)
			{
				do_read();
			}
		});
	}

	tcp::socket socket_;
	enum { max_length = 1024 };
	char data_[max_length];
};

TcpServer::TcpServer(boost::asio::io_service& io_service, short port, LinkUpNode* node)
	: acceptor_(io_service, tcp::endpoint(tcp::v4(), port)),
	socket_(io_service)
{
	do_accept();
}

void TcpServer::do_accept()
{
	acceptor_.async_accept(socket_,
		[this](boost::system::error_code ec)
	{
		if (!ec)
		{
			std::cout << "new connection" << std::endl;
			std::make_shared<session>(std::move(socket_))->start();
		}

		do_accept();
	});
}

