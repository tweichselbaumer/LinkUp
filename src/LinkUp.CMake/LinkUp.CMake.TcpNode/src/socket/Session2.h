#ifndef _SESSION_H
#define _SESSION_H

#include <boost/asio.hpp>
#include "TcpServer.h"
#include <cstdlib>
#include <iostream>

using boost::asio::ip::tcp;
using namespace std;

class tcp_server;

class Session
	: public std::enable_shared_from_this<Session>
{
public:
	Session(tcp::socket socket, TcpServer* server, LinkUpNode* node);
	void start();
	void read();
	void write();
	~Session();
	boost::asio::ip::address address_;
	uint16_t port_;
private:
	enum { max_length = 1024 * 100 };

	uint8_t* dataIn_;
	uint8_t* dataOut1_;
	uint8_t* dataOut2_;
	uint64_t totalsend = 0;

	uint32_t length1_ = 0;
	uint32_t length2_ = 0;

	boost::mutex mtx;

	tcp::socket socket_;
	TcpServer* server_;
	LinkUpNode * node_;
};
#endif
