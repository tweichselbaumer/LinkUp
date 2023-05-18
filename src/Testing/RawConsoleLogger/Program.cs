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

using LinkUp.Cs.Datagram;
using LinkUp.Raw;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace RawConsoleLogger
{
   internal class Program
   {
      private static void ArqProtcol_ReveivedDatagram(IDatagramProtocol sender, Datagram datagram)
      {
         Logger logger = LogManager.GetCurrentClassLogger();
         //logger.Debug("Received Raw Packet - Size: {0} Content: {0}", packet.Length, string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
      }

      private static void Connector_ReveivedPacket(LinkUpConnector connector, LinkUpPacket packet)
      {
         Logger logger = LogManager.GetCurrentClassLogger();
         //logger.Debug("Received Raw Packet - Size: {0} Content: {0}", packet.Length, string.Join(" ", packet.Data.Select(b => string.Format("{0:X2} ", b))));
      }

      private static void Main(string[] args)
      {
         var config = new LoggingConfiguration();
         var consoleTarget = new ColoredConsoleTarget
         {
            Name = "console",
            Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",
         };

         config.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget, "*");
         LogManager.Configuration = config;

         Logger logger = LogManager.GetCurrentClassLogger();
         logger.Info("Start LinkUp Raw Console Logger!");

         config.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget, "*");

         LinkUpSerialPortConnector connector = new LinkUpSerialPortConnector("COM10", 500000 /*460800*/);
         //connector.ReveivedPacket += Connector_ReveivedPacket;

         ARQProtocol arqProtcol = new ARQProtocol(connector);
         arqProtcol.ReveivedDatagram += ArqProtcol_ReveivedDatagram;

         System.Timers.Timer timer;
         timer = new System.Timers.Timer(1000);
         timer.Elapsed += (object? sender, System.Timers.ElapsedEventArgs e) =>
         {
            logger.Info("Send {0:0.00}kb/s - Receive {1:0.00}kb/s", connector.SentBytesPerSecond / 1024.0, connector.ReceivedBytesPerSecond / 1024.0);
         };
         timer.Start();

         Task.Run(() =>
         {
            int i = 0;
            while (true)
            {
               Random rnd = new Random();
               byte[] data = new byte[1024];
               data[0] = (byte)i++;
               arqProtcol.Send(new Datagram(data));
            }
         });

         Console.ReadKey();
      }
   }
}