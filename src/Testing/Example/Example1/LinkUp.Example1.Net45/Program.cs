using LinkUp.Raw;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace LinkUp.Example1.Net45
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            byte[] data = new byte[] { 0x99, 0x0A, 0x01, 0xAA, 0x01, 0xAA, 0xAA, 0x01, 0xAA, 0x05, 0x55,
                                       0x8A, 0x55, 0xB9, 0x55, 0x75, 0x01, 0x02, 0x6F, 0xFF, 0x99, 0x99,
                                       0x0A, 0x01, 0xAA, 0x01, 0xAA, 0xAA, 0x01, 0xAA, 0x05, 0x55, 0x8A,
                                       0x55, 0xB9, 0x55, 0x75, 0x01, 0x02, 0x6F, 0xFF, 0x99 };

            BlockingCollection<byte[]> col1 = new BlockingCollection<byte[]>();
            BlockingCollection<byte[]> col2 = new BlockingCollection<byte[]>();

            LinkUpMemoryConnector slaveToMaster = new LinkUpMemoryConnector(col1, col2);
            LinkUpMemoryConnector masterToSlave = new LinkUpMemoryConnector(col2, col1);

            slaveToMaster.ReveivedPacket += SlaveToMaster_ReveivedPacket;
            masterToSlave.ReveivedPacket += MasterToSlave_ReveivedPacket;

            slaveToMaster.SendPacket(new LinkUpPacket() { Data = data });
            masterToSlave.SendPacket(new LinkUpPacket() { Data = data });

            Console.Read();
        }

        private static void MasterToSlave_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("- Slave from Master:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }

        private static void SlaveToMaster_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("- Master from Slave:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
                Console.ResetColor();
            }
        }
    }
}