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
	enum { max_length = 1024 * 100 };

	uint8_t dataIn_[max_length];
	uint8_t dataOut_[max_length];
	uint64_t totalsend = 0;

	bool read_done = true;

	tcp::socket socket_;
	tcp_server* server_;
	LinkUpNode * node_;
};
#endif
