#ifndef TCP_SERVER_H
#define TCP_SERVER_H

#include <boost/asio.hpp>
#include <LinkUpRaw.h>
#include <LinkUpNode.h>

using boost::asio::ip::tcp;
using namespace std;

class session;

class tcp_server
{
private:
	tcp::acceptor acceptor_;
	tcp::socket socket_;
	uint8_t connections_;
	uint8_t maxConnections_;
	LinkUpNode * node_;
	void do_accept();
public:
	tcp_server(boost::asio::io_service & io_service, short port, LinkUpNode * node, uint8_t maxConnections);
	void removeSession(session* session);
};

#endif