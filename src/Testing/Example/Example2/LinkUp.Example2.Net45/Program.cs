/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2023 Thomas Weichselbaumer
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 ********************************************************************************/

using LinkUp.Raw;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace LinkUp.Example2.Net45
{
	internal class Program
	{
		private const int DATA_BAUD = 250000;
		private const string DATA_PORT = "COM6";
		private const int DEBUG_BAUD = 250000;
		private const string DEBUG_PORT = "COM3";
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

		private static void Main(string[] args)
		{
			byte[] data = new byte[] { 0x99, 0x0A, 0x01, 0xAA, 0x01, 0xAA, 0xAA, 0x01, 0xAA, 0x05, 0x55,
									   0x8A, 0x55, 0xB9, 0x55, 0x75, 0x01, 0x02, 0x6F, 0xFF, 0x99, 0x99,
									   0x0A, 0x01, 0xAA, 0x01, 0xAA, 0xAA, 0x01, 0xAA, 0x05, 0x55, 0x8A,
									   0x55, 0xB9, 0x55, 0x75, 0x01, 0x02, 0x6F, 0xFF, 0x99 };

			watch = new Stopwatch();
			watch.Start();

			SerialPort port = new SerialPort(DEBUG_PORT, DEBUG_BAUD);
			port.Open();
			port.ReadExisting();
			port.DataReceived += Port_DataReceived;

			LinkUpSerialPortConnector connector = new LinkUpSerialPortConnector(DATA_PORT, DATA_BAUD);
			connector.ReveivedPacket += Connector_ReveivedPacket;

			while (true)
			{
				watch.Restart();
				connector.SendPacket(new LinkUpPacket() { Data = data });
				Console.WriteLine("{0} - Send", watch.ElapsedTicks * 1000 / Stopwatch.Frequency);
				Thread.Sleep(1000);
			}

			Console.Read();
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