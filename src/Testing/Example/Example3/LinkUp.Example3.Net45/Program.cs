using LinkUp.Node;
using LinkUp.Raw;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Example3.Net45
{
    internal class Program
    {
        private const int DATA_BAUD = 250000;
        private const string DATA_PORT = "COM3";
        private const int DEBUG_BAUD = 115200;
        private const string DEBUG_PORT = "";
        private static SerialPort port;
        private static Stopwatch watch;

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

        private static void Main(string[] args)
        {
            watch = new Stopwatch();
            watch.Start();

            if (!string.IsNullOrEmpty(DEBUG_PORT))
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            if (port == null || !port.IsOpen)
                            {
                                port = new SerialPort(DEBUG_PORT, DEBUG_BAUD);
                                port.Open();
                                port.ReadExisting();
                                port.DataReceived += Port_DataReceived;
                            }
                        }
                        catch (Exception)
                        {
                            port = null;
                        }
                        Thread.Sleep(100);
                    }
                });
            }

            LinkUpSerialPortConnector connector = new LinkUpSerialPortConnector(DATA_PORT, DATA_BAUD);
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


                foreach (LinkUpPrimitiveLabel<int> value in node.Labels.Where(c => c is LinkUpPrimitiveLabel<int>))
                {
                    try
                    {
                        value.Value = 100;
                    }
                    catch (Exception ex) { Debug.WriteLine(ex.ToString()); }

                }

                Console.Read();

            }
        }

        private static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (sender is SerialPort)
            {
                SerialPort port = (SerialPort)sender;
                lock (Console.Out)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("{0} - Debug:\t{1}", watch.ElapsedTicks * 1000 / Stopwatch.Frequency, port.ReadExisting());
                    Console.ResetColor();
                }
            }
        }
    }
}