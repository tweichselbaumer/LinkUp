using LinkUp.Raw;
using System;
using System.Linq;
using System.Threading;

namespace LinkUp.Example5.Net45
{
    internal class Program
    {
        private static void Connector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("- Master from Slave:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }

        private static void Main(string[] args)
        {
            byte[] data = new byte[] { 0x99, 0x0A, 0x01, 0xAA, 0x01, 0xAA, 0xAA, 0x01, 0xAA, 0x05, 0x55,
                                       0x8A, 0x55, 0xB9, 0x55, 0x75, 0x01, 0x02, 0x6F, 0xFF, 0x99, 0x99,
                                       0x0A, 0x01, 0xAA, 0x01, 0xAA, 0xAA, 0x01, 0xAA, 0x05, 0x55, 0x8A,
                                       0x55, 0xB9, 0x55, 0x75, 0x01, 0x02, 0x6F, 0xFF, 0x99 };

            LinkUpNamedPipeConnector connector = new LinkUpNamedPipeConnector("linkup", LinkUpNamedPipeConnector.Mode.Server);

            connector.ReveivedPacket += Connector_ReveivedPacket; ;

            while (true)
            {
                connector.SendPacket(new LinkUpPacket() { Data = data });
                Thread.Sleep(1000);
            }

            Console.Read();
        }
    }
}