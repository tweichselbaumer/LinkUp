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
	~session();
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

	bool read_done = true;

	

	tcp::socket socket_;
	tcp_server* server_;
	LinkUpNode * node_;
};
#endif
