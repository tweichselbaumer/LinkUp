using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Testing.dotnet
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            StaticTest2();
        }

        private static void MasterToSlave_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("- Master from Slave:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            Console.ResetColor();
        }

        private static void MasterToSuper_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("- Master from Super:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            Console.ResetColor();
        }

        private static void SlaveToMaster_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("- Slave from Master:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            Console.ResetColor();
        }

        private static void StaticTest1()
        {
            byte[] data = new byte[] { 0x99, 0xa, 0x1, 0xAA, 0x1, 0xAA, 0xAA, 0x1, 0xAA, 0x5, 0x55, 0x8A, 0x55, 0xB9, 0x55, 0x75, 0x1, 0x2, 0x6F, 0xFF, 0x99, 0x99, 0xa, 0x1, 0xAA, 0x1, 0xAA, 0xAA, 0x1, 0xAA, 0x5, 0x55, 0x8A, 0x55, 0xB9, 0x55, 0x75, 0x1, 0x2, 0x6F, 0xFF, 0x99 };

            BlockingCollection<byte[]> col1 = new BlockingCollection<byte[]>();
            BlockingCollection<byte[]> col2 = new BlockingCollection<byte[]>();

            LinkUpMemoryConnector c1 = new LinkUpMemoryConnector(col1, col2);
            LinkUpMemoryConnector c2 = new LinkUpMemoryConnector(col2, col1);

            c1.ReveivedPacket += MasterToSlave_ReveivedPacket;
            c2.ReveivedPacket += SlaveToMaster_ReveivedPacket;

            c1.SendPacket(new LinkUpPacket() { Data = data });

            Console.Read();
        }

        private static void StaticTest2()
        {
            BlockingCollection<byte[]> col1 = new BlockingCollection<byte[]>();
            BlockingCollection<byte[]> col2 = new BlockingCollection<byte[]>();

            BlockingCollection<byte[]> col3 = new BlockingCollection<byte[]>();
            BlockingCollection<byte[]> col4 = new BlockingCollection<byte[]>();

            LinkUpMemoryConnector masterToSlave = new LinkUpMemoryConnector(col1, col2);
            LinkUpMemoryConnector slaveToMaster = new LinkUpMemoryConnector(col2, col1);

            LinkUpMemoryConnector masterToSuper = new LinkUpMemoryConnector(col3, col4);
            LinkUpMemoryConnector superToMaster = new LinkUpMemoryConnector(col4, col3);

            masterToSlave.ReveivedPacket += MasterToSlave_ReveivedPacket;
            slaveToMaster.ReveivedPacket += SlaveToMaster_ReveivedPacket;
            masterToSuper.ReveivedPacket += MasterToSuper_ReveivedPacket;
            superToMaster.ReveivedPacket += SuperToMaster_ReveivedPacket;

            LinkUpNode super = new LinkUpNode();
            super.AddSubNode(superToMaster);
            super.Name = "super";

            LinkUpNode master = new LinkUpNode();
            master.AddSubNode(masterToSlave);
            master.MasterConnector = masterToSuper;
            master.Name = "master";

            LinkUpNode slave = new LinkUpNode();
            slave.MasterConnector = slaveToMaster;
            slave.Name = "slave";

            LinkUpPrimitiveLabel<int> value2 = master.AddLabel<LinkUpPrimitiveLabel<int>>("value2");
            LinkUpPrimitiveLabel<int> value1 = slave.AddLabel<LinkUpPrimitiveLabel<int>>("value1");
            
            value1.Value = 10;
            value2.Value = 1011;

            Console.WriteLine("Continue after init!");
            Console.ReadLine();

            LinkUpPrimitiveLabel<int> value1Ref = (LinkUpPrimitiveLabel<int>)master.Labels.FirstOrDefault(c => c.Name == "master/slave/value1");
            Console.WriteLine("value1Ref: " + value1Ref.Value);

            value1Ref.Value = 123;
            Console.WriteLine("value1: " + value1.Value);
            value1Ref.Value = 10;
            Console.WriteLine("value1: " + value1.Value);

            LinkUpPrimitiveLabel<int> value1RefSuper = (LinkUpPrimitiveLabel<int>)super.Labels.FirstOrDefault(c => c.Name == "super/master/slave/value1");
            Console.WriteLine("value1RefSuper: " + value1RefSuper.Value);

            value1RefSuper.Value = 123;
            Console.WriteLine("value1: " + value1.Value);
            value1RefSuper.Value = 10;
            Console.WriteLine("value1: " + value1.Value);

            LinkUpPrimitiveLabel<int> value2RefSuper = (LinkUpPrimitiveLabel<int>)super.Labels.FirstOrDefault(c => c.Name == "super/master/value2");
            Console.WriteLine("value2RefSuper: " + value2RefSuper.Value);

            value2RefSuper.Value = 1245;
            Console.WriteLine("value2: " + value2.Value);
            value2RefSuper.Value = 1152;
            Console.WriteLine("value2: " + value2.Value);

            Console.WriteLine("done");
            Console.ReadLine();
        }

        private static void SuperToMaster_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("- Super from Master:\n\t{0}", string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
            Console.ResetColor();
        }
    }
}