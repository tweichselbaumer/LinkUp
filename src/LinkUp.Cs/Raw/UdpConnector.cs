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

using System.Net;
using System.Net.Sockets;

namespace LinkUp.Cs.Raw
{
   public class UdpConnector : Connector
   {
      private bool _IsRunning = true;
      private Task _Task;
      private UdpClient _UdpClient;

      public UdpConnector(IPAddress sourceAddress, IPAddress destinationAddress, int sourcePort, int destinationPort)
      {
         _Task = Task.Run(() =>
         {
            while (_IsRunning)
            {
               try
               {
                  if (_UdpClient == null)
                  {
                     _UdpClient = new UdpClient(new IPEndPoint(sourceAddress, sourcePort));
                     _UdpClient.Connect(new IPEndPoint(destinationAddress, destinationPort));
                  }
                  IPEndPoint endPoint = new IPEndPoint(destinationAddress, destinationPort);
                  byte[] data = _UdpClient.Receive(ref endPoint);
                  OnDataReceived(data);
               }
               catch (Exception)
               {
                  _UdpClient.Close();
                  _UdpClient = null;
               }
            }
         });
      }

      protected override void SendData(byte[] data)
      {
         if (_UdpClient == null)
         {
            Thread.Sleep(200);
         }
         if (_UdpClient != null)
         {
            _UdpClient.Send(data, data.Length);
         }
         else
         {
            throw new Exception("Not connected.");
         }
      }

      public override void Dispose()
      {
         _IsRunning = false;
         if (_UdpClient != null)
         {
            _UdpClient.Close();
         }
         _Task.Wait();
      }
   }
}