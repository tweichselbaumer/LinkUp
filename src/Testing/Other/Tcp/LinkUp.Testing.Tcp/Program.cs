using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Testing.Tcp
{
    internal class Program
    {
        private static int count = 0;

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
            //        Console.WriteLine("- Reveived (-):\n\t{1}", count++, string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
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
        }

        private static void Main(string[] args)
        {
            using (LinkUpTcpClientConnector connector = new LinkUpTcpClientConnector(IPAddress.Parse("127.0.0.1"), 3000))
            {
                connector.ReveivedPacket += ClientToServer_ReveivedPacket;
                connector.SentPacket += ClientToServer_SentPacket;

                LinkUpNode node = new LinkUpNode();
                node.Name = "leaf";
                node.AddSubNode(connector);

                bool running = true;

                Task.Run(() =>
                {
                    while (running)
                    {
                        try
                        {
                            if (node.Labels.Count == 1000)
                            {
                                foreach (LinkUpLabel lab in node.Labels)
                                {
                                    if (lab is LinkUpPrimitiveLabel<Int32>)
                                    {
                                        lock (Console.Out)
                                        {
                                            int value = (lab as LinkUpPrimitiveLabel<Int32>).Value;
                                            Console.ResetColor();
                                            //Console.WriteLine("{0}: {1}", lab.Name, value);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            lock (Console.Out)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(e.Message);
                                Console.ResetColor();
                            }
                        }

                        Thread.Sleep(10);
                    }
                });

                Console.Read();
                running = false;
                connector.Dispose();
            }
        }
    }
}