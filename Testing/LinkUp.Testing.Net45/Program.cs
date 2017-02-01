using LinkUp.Raw;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Testing.dotnet
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            byte[] data = new byte[] { 0x99, 0xa, 0x1, 0xAA, 0x1, 0xAA, 0xAA, 0x1, 0xAA, 0x5, 0x55, 0x8A, 0x55, 0xB9, 0x55, 0x75, 0x1, 0x2, 0x6F, 0xFF, 0x99, 0x99, 0xa, 0x1, 0xAA, 0x1, 0xAA, 0xAA, 0x1, 0xAA, 0x5, 0x55, 0x8A, 0x55, 0xB9, 0x55, 0x75, 0x1, 0x2, 0x6F, 0xFF, 0x99 };

            BlockingCollection<byte[]> col1 = new BlockingCollection<byte[]>();
            BlockingCollection<byte[]> col2 = new BlockingCollection<byte[]>();

            LinkUpMemoryConnector c1 = new LinkUpMemoryConnector(col1, col2);
            LinkUpMemoryConnector c2 = new LinkUpMemoryConnector(col2, col1);

            c1.ReveivedPacket += C1_ReveivedPacket;
            c2.ReveivedPacket += C2_ReveivedPacket;

            c1.SendPacket(new LinkUpPacket() { Data = data });

            Console.Read();
        }

        private static void C2_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Console.WriteLine("- C2:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            connector.SendPacket(packet);
        }

        private static void C1_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Console.WriteLine("- C1:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
        }
    }
}