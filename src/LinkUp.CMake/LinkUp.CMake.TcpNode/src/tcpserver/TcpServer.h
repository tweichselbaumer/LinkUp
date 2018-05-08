#include <boost/asio.hpp>
#include <LinkUpRaw.h>
#include <LinkUpNode.h>

using boost::asio::ip::tcp;
using namespace std;


class TcpServer
{
private:
	tcp::acceptor acceptor_;
	tcp::socket socket_;

	void do_accept();
public:
	TcpServer(boost::asio::io_service & io_service, short port, LinkUpNode * node);
};