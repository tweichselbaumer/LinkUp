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
			node_->progress(dataIn_, length, 10000, true);
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
		if (read_done) {
			read_done = false;
			read();
		}

		mtx.lock();

		length2_ = length1_;
		memcpy(dataOut2_, dataOut1_, length1_);

		if (length2_ == 0)
		{
			boost::this_thread::sleep_for(boost::chrono::milliseconds(1));
		}

		boost::asio::async_write(socket_, boost::asio::buffer(dataOut2_, length2_),
			[this, self](boost::system::error_code ec, std::size_t /*length*/)
		{
			if (ec == 0)
			{
				boost::this_thread::sleep_for(boost::chrono::milliseconds(0));
				start();
			}
			else {
				std::cout << "Closed connection [" << socket_.remote_endpoint().address().to_string() << ":" << socket_.remote_endpoint().port() << "]" << std::endl;
				server_->removeSession(this);
				return;
			}
		});

		length1_ = node_->getRaw(dataOut1_, max_length);

		mtx.unlock();
	}
	catch (std::exception& e)
	{
		std::cerr << "Exception in thread: " << e.what() << "\n";
	}
}

