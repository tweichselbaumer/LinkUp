using LinkUp.Raw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Example6.Net45
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] data = new byte[] { 0x99, 0x0A, 0x01, 0xAA, 0x01, 0xAA, 0xAA, 0x01, 0xAA, 0x05, 0x55,
                                       0x8A, 0x55, 0xB9, 0x55, 0x75, 0x01, 0x02, 0x6F, 0xFF, 0x99, 0x99,
                                       0x0A, 0x01, 0xAA, 0x01, 0xAA, 0xAA, 0x01, 0xAA, 0x05, 0x55, 0x8A,
                                       0x55, 0xB9, 0x55, 0x75, 0x01, 0x02, 0x6F, 0xFF, 0x99 };

            LinkUpNamedPipeConnector masterToSlave = new LinkUpNamedPipeConnector("linkup", LinkUpNamedPipeConnector.Mode.Server);
            LinkUpNamedPipeConnector slaveToMaster = new LinkUpNamedPipeConnector("linkup", LinkUpNamedPipeConnector.Mode.Client);

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
