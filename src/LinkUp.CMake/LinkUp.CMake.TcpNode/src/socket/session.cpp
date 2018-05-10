#include "tcp_server.h"
#include "session.h"

#include <cstdlib>
#include <iostream>


session::session(tcp::socket socket, tcp_server* server, LinkUpNode* node)
	: socket_(std::move(socket))
{
	server_ = server;
	node_ = node;
}

void session::read()
{
	auto self(shared_from_this());
	socket_.async_read_some(boost::asio::buffer(data_, max_length),
		[this, self](boost::system::error_code ec, std::size_t length)
	{
		if (ec == 0)
		{
			node_->progress(data_, length);
			read_done = true;
		}
		else {
			std::cout << "Closed connection [" << socket_.remote_endpoint().address().to_string() << ":" << socket_.remote_endpoint().port() << "]" << std::endl;
			server_->removeSession(this);
			return;
		}
	});
}


void session::start()
{
	auto self(shared_from_this());
	boost::system::error_code error;

	try
	{

		size_t length = 0;
		if (read_done) {
			read_done = false;
			read();
		}
		node_->progress(data_, 0);
		length = node_->getRaw(data_, max_length);

		if (length > 0) {
			for (uint16_t i = 0; i < length; i++)
			{
				std::cout << "0x";
				cout.setf(ios::hex, ios::basefield);
				std::cout << (int)data_[i];
				std::cout << " ";
			}
			std::cout << std::endl;
		}

		boost::asio::async_write(socket_, boost::asio::buffer(data_, length),
			[this, self](boost::system::error_code ec, std::size_t /*length*/)
		{
			if (ec == 0)
			{
				start();
			}
			else {
				std::cout << "Closed connection [" << socket_.remote_endpoint().address().to_string() << ":" << socket_.remote_endpoint().port() << "]" << std::endl;
				server_->removeSession(this);
				return;
			}
		});


	}
	catch (std::exception& e)
	{
		std::cerr << "Exception in thread: " << e.what() << "\n";
	}
}

