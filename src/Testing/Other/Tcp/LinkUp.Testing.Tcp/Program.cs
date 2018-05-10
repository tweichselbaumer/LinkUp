using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Testing.Tcp
{
    class Program
    {
        static void Main(string[] args)
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
                            foreach (LinkUpLabel lab in node.Labels)
                            {
                                if (lab is LinkUpPrimitiveLabel<Int32>)
                                {
                                    
                                    lock (Console.Out)
                                    {
                                        int value = (lab as LinkUpPrimitiveLabel<Int32>).Value;
                                        Console.ResetColor();
                                        Console.WriteLine("{0}: {1}", lab.Name, value);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            lock (Console.Out)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(e.ToString());
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

        private static void ClientToServer_SentPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("- Sent:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }

        private static void ClientToServer_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("- Reveived:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }

        }
    }
}
