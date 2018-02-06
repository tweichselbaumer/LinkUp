using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LinkUp.Explorer.TestNode
{
    class Program
    {
        static void Main(string[] args)
        {
            LinkUpUdpConnector apiConnector = new LinkUpUdpConnector(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1"), 1000, 2000);

            apiConnector.ReveivedPacket += ReveivedPacket;
            apiConnector.SentPacket += SentPacket;

            LinkUpNode masterNode = new LinkUpNode();
            masterNode.Name = "master";
            masterNode.MasterConnector = apiConnector;
            masterNode.AddLabel<LinkUpPrimitiveLabel<int>>("val1");

            LinkUpNode slave1 = CreateNode(masterNode, "slave1");
            LinkUpNode slave2 = CreateNode(masterNode, "slave1");

            LinkUpNode slaveSlave1 = CreateNode(slave1, "slaveSlave1");

            Console.Read();

        }

        private static LinkUpNode CreateNode(LinkUpNode masterNode, string name)
        {
            LinkUpNode node = new LinkUpNode();
            node.Name = name;

            BlockingCollection<byte[]> col1 = new BlockingCollection<byte[]>();
            BlockingCollection<byte[]> col2 = new BlockingCollection<byte[]>();

            LinkUpMemoryConnector slaveToMaster = new LinkUpMemoryConnector(col1, col2);
            LinkUpMemoryConnector masterToSlave = new LinkUpMemoryConnector(col2, col1);

            node.MasterConnector = slaveToMaster;
            masterNode.AddSubNode(masterToSlave);

            node.AddLabel<LinkUpPrimitiveLabel<int>>("val1");
            node.AddLabel<LinkUpPrimitiveLabel<int>>("val2");

            return node;
        }

        private static void SentPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("- Sent Data:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }

        private static void ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("- Received Data:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }
    }
}
