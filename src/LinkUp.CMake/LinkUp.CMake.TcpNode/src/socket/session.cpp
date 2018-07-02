#include "tcp_server.h"
#include "session.h"

#include <cstdlib>
#include <iostream>


session::session(tcp::socket socket, tcp_server* server, LinkUpNode* node)
	: socket_(std::move(socket))
{
	server_ = server;
	node_ = node;
	address_ = socket_.remote_endpoint().address();
	port_ = socket_.remote_endpoint().port();

	dataIn_ = (uint8_t*)calloc(max_length, sizeof(uint8_t));
	dataOut1_ = (uint8_t*)calloc(max_length, sizeof(uint8_t));
	dataOut2_ = (uint8_t*)calloc(max_length, sizeof(uint8_t));

	node->reset();
	length1_ = node_->getRaw(dataOut1_, max_length);
}

void session::read()
{
	auto self(shared_from_this());
	socket_.async_read_some(boost::asio::buffer(dataIn_, max_length),
		[this, self](boost::system::error_code ec, std::size_t length)
	{
		if (ec == 0)
		{
			node_->progress(dataIn_, length, 10000, true);
			read_done = true;
		}
		else {
			server_->removeSession(this);
			return;
		}
	});
}

session::~session() 
{
	free(dataIn_);
	free(dataOut1_);
	free(dataOut2_);
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

		uint8_t* pTemp = dataOut2_;
		dataOut2_ = (uint8_t*)dataOut1_;
		dataOut1_ = pTemp;

		if (length2_ == 0)
		{
			boost::this_thread::sleep_for(boost::chrono::milliseconds(1));
		}

		boost::asio::async_write(socket_, boost::asio::buffer(dataOut2_, length2_),
			[this, self](boost::system::error_code ec, std::size_t length)
		{
			if (ec == 0)
			{
				start();
			}
			else
			{
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

