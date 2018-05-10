#ifndef SESSION_H
#define SESSION_H

#include <boost/asio.hpp>

using boost::asio::ip::tcp;
using namespace std;

class tcp_server;

class session
	: public std::enable_shared_from_this<session>
{
public:
	session(tcp::socket socket, tcp_server* server, LinkUpNode* node);
	void start();
	void read();

private:
	enum { max_length = 1024 };

	uint8_t data_[max_length];

	bool read_done = true;

	tcp::socket socket_;
	tcp_server* server_;
	LinkUpNode * node_;
};
#endif
