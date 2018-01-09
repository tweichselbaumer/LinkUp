using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LinkUp.Example8.Client.Net45
{
    class Program
    {
        static void Main(string[] args)
        {
            LinkUpUdpConnector clientToServer = new LinkUpUdpConnector(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1"), 2000, 1000);
            clientToServer.ReveivedPacket += ClientToServer_ReveivedPacket;

            LinkUpNode node = new LinkUpNode();
            node.Name = "leaf";
            node.MasterConnector = clientToServer;
            LinkUpPrimitiveLabel<int> val1 = node.AddLabel<LinkUpPrimitiveLabel<int>>("val1");
            val1.Value = 1213;

            Console.Read();
        }

        private static void ClientToServer_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("- Client from Server:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            
        }
    }
}
