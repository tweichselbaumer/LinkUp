using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Testing.Tcp
{
    internal class Program
    {
        private static Stopwatch stopWatch;
        private static int count = 0;
        private static long bytes;

        private static void ClientToServer_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            //lock (Console.Out)
            //{
            //    Console.ForegroundColor = ConsoleColor.Yellow;
            //    if (packet.Data.Length > 2 && packet.Data[0] == 0x01 && packet.Data[1] == 0x9)
            //    {
            //       Console.WriteLine("- Reveived ({0}):\n\t{1}", count++, string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            //    }
            //    else
            //    {
            //Console.WriteLine("- Reveived:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            //    }
            //    Console.ResetColor();
            //}
        }

        private static void ClientToServer_SentPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            //lock (Console.Out)
            //{
            //    Console.ForegroundColor = ConsoleColor.Magenta;
            //    Console.WriteLine("- Sent:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            //    Console.ResetColor();
            //}
            //lock (Console.Out)
            //{
            //    if (packet.Data.Length > 2 && packet.Data[0] == 0x02 && packet.Data[1] == 0x9)
            //    {
            //Console.WriteLine("- Sent: {0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            //    }
            //}
        }

        private static void Main(string[] args)
        {
            using (LinkUpTcpClientConnector connector = new LinkUpTcpClientConnector(IPAddress.Parse("127.0.0.1"), 3000))
            {
                connector.ReveivedPacket += ClientToServer_ReveivedPacket;
                connector.SentPacket += ClientToServer_SentPacket;
                connector.ConnectivityChanged += Connector_ConnectivityChanged;

                LinkUpNode node = new LinkUpNode();
                node.Name = "leaf";
                node.AddSubNode(connector);

                LinkUpEventLabel eventLabel = node.GetLabelByName<LinkUpEventLabel>("leaf/test/label_event");
                eventLabel.Subscribe();
                eventLabel.Fired += Program_Fired;

                Console.Read();
                connector.Dispose();
            }
        }

        private static void Connector_ConnectivityChanged(LinkUpConnector connector, LinkUpConnectivityType connectivity)
        {
            Console.WriteLine(connectivity);
        }

        private static void Program_Fired(LinkUpEventLabel label, byte[] data)
        {
            if (count == 0)
            {
                stopWatch = new Stopwatch();
                stopWatch.Start();
            }
            count++;
            bytes += data.Length;
            if (count % 1000 == 0)
            {
                stopWatch.Restart();
                count = 0;
                bytes = 0;
            }
            else
            {
                Console.WriteLine("{0:0.0} events/s\t{1:0.0} KB/s\t{2:0.0} MBit/s", ((double)count) / stopWatch.ElapsedMilliseconds * 1000, ((double)bytes) / stopWatch.ElapsedMilliseconds * 1000 / 1024, ((double)bytes) / stopWatch.ElapsedMilliseconds * 1000 / 1024 /1024*8);
            }
            //onsole.WriteLine("- EVENT ({0}): {1}", label.Name, data.Length/*string.Join(" ", data.Select(b => string.Format("{0:X2} ", b)))*/);
        }
    }
}