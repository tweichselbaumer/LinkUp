﻿using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Example7.Net45
{
    class Program
    {
        private static Stopwatch watch;
        static void Main(string[] args)
        {
            watch = new Stopwatch();
            watch.Start();

            LinkUpNamedPipeConnector connector = new LinkUpNamedPipeConnector("linkup", LinkUpNamedPipeConnector.Mode.Server);
            connector.ReveivedPacket += Connector_ReveivedPacket;
            connector.SentPacket += Connector_SentPacket;

            LinkUpNode node = new LinkUpNode();
            node.Name = "net45";
            node.AddSubNode(connector);

            Thread.Sleep(5000);

            while (true)
            {

                foreach (LinkUpPrimitiveLabel<int> value in node.Labels.Where(c => c is LinkUpPrimitiveLabel<int>))
                {
                    try
                    {
                        Console.WriteLine(string.Format("{0}: {1}", value.Name, value.Value));
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
                }


                Thread.Sleep(5000);

                foreach (LinkUpPrimitiveLabel<int> value in node.Labels.Where(c => c is LinkUpPrimitiveLabel<int>))
                {
                    try
                    {
                        value.Value = 100;
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }

                }

            }
            Console.Read();
        }
        private static void Connector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0} - Receive:\n\t{1}", watch.ElapsedTicks * 1000 / Stopwatch.Frequency, string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }

        private static void Connector_SentPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("{0} - Sent:\n\t{1}", watch.ElapsedTicks * 1000 / Stopwatch.Frequency, string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }
    }
}
