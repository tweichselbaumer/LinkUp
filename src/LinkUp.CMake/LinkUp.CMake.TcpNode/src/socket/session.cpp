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
	socket_.async_read_some(boost::asio::buffer(dataIn_, max_length),
		[this, self](boost::system::error_code ec, std::size_t length)
	{
		if (ec == 0)
		{
			//cout << "in: " << length << endl;
			node_->progress(dataIn_, length, 100000, true);
			read_done = true;
		}
		else {
			/*std::cout << "Closed connection [" << socket_.remote_endpoint().address().to_string() << ":" << socket_.remote_endpoint().port() << "]" << std::endl;
			server_->removeSession(this);*/
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

		length = node_->getRaw(dataOut_, max_length);

		totalsend += length;
		//std::cout << totalsend << std::endl;
		if (length == 0) {
			//	for (uint16_t i = 0; i < length; i++)
			//	{
			//		std::cout << "0x";
			//		cout.setf(ios::hex, ios::basefield);
			//		std::cout << (int)data_[i];
			//		std::cout << " ";
			//	}
			//	std::cout << std::endl;
			//boost::this_thread::sleep_for(boost::chrono::milliseconds(1));
		}
		else {
			//std::cout << "out: " << length << std::endl;
		}

		boost::asio::async_write(socket_, boost::asio::buffer(dataOut_, length),
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

