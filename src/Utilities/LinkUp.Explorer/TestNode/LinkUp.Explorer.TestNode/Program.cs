using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Timers;

namespace LinkUp.Explorer.TestNode
{
    internal class Program
    {
        private static LinkUpNode masterNode = new LinkUpNode();

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

            node.AddLabel<LinkUpPropertyLabel<int>>("val1");
            node.AddLabel<LinkUpPropertyLabel<int>>("val2");

            return node;
        }

        private static void Main(string[] args)
        {
            LinkUpUdpConnector apiConnector = new LinkUpUdpConnector(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1"), 1000, 2000);

            apiConnector.ReveivedPacket += ReveivedPacket;
            apiConnector.SentPacket += SentPacket;

            masterNode.Name = "master";
            masterNode.MasterConnector = apiConnector;
            masterNode.AddLabel<LinkUpPropertyLabel<int>>("val1");

            LinkUpNode slave1 = CreateNode(masterNode, "slave1");
            LinkUpNode slave2 = CreateNode(masterNode, "slave2");

            LinkUpNode slaveSlave1 = CreateNode(slave1, "slave11");

            Timer t = new Timer(500);
            t.Elapsed += T_Elapsed;
            t.Start();

            Console.Read();
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

        private static void SentPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("- Sent Data:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }

        private static void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (LinkUpPropertyLabel<int> label in masterNode.Labels.Where(c => c is LinkUpPropertyLabel<int>))
            {
                label.Value = new Random((int)DateTime.Now.Ticks).Next(1, 100);
            }
        }
    }
}