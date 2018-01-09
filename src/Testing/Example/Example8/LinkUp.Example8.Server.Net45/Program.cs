using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LinkUp.Example8.Server.Net45
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            LinkUpUdpConnector serverToClient = new LinkUpUdpConnector(IPAddress.Parse("127.0.0.1"), IPAddress.Parse("127.0.0.1"), 1000, 2000);
            serverToClient.ReveivedPacket += ServerToClient_ReveivedPacket;

            LinkUpNode node = new LinkUpNode();
            node.Name = "root";
            node.AddSubNode(serverToClient);
            while (node.Labels.Count < 1) { }
            LinkUpPrimitiveLabel<int> val1 = node.Labels[0] as LinkUpPrimitiveLabel<int>;
            for (int i = 0; i < 100; i++)
            {
                watch.Restart();
                int value = val1.Value;
                Console.WriteLine("GET {0}: {1} in {2} µs", val1.Name, value, watch.ElapsedTicks * 1000 * 1000 / Stopwatch.Frequency);
                Console.Read();

                watch.Restart();
                val1.Value = value * i;
                Console.WriteLine("SET {0}: {1} in {2} µs", val1.Name, value * i, watch.ElapsedTicks * 1000 * 1000 / Stopwatch.Frequency);
                Console.Read();
            }
        }

        private static void ServerToClient_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("- Server from Client:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            Console.ResetColor();

        }
    }
}
