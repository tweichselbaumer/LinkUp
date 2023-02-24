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

using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

namespace LinkUp.Raw
{
   public class LinkUpTcpClientConnector : LinkUpConnector
   {
      private const int maxRead = 1024 * 100;
      private bool _IsRunning = true;
      private BlockingCollection<byte[]> _QueueIn = new BlockingCollection<byte[]>();
      private BlockingCollection<byte[]> _QueueOut = new BlockingCollection<byte[]>();
      private Task _TaskIn;
      private Task _TaskOut;
      private TcpClient _TcpClient;
      private byte[] data = new byte[maxRead];

      public LinkUpTcpClientConnector(IPAddress destinationAddress, int destinationPort)
      {
         _TaskIn = Task.Factory.StartNew(() =>
         {
            byte[] buffer = new byte[maxRead * 50];
            while (_IsRunning)
            {
               try
               {
                  byte[] data;

                  int size = 0;
                  int count = 0;

                  while (_QueueIn.TryTake(out data, 10))
                  {
                     Array.Copy(data, 0, buffer, size, data.Length);
                     size += data.Length;
                     count++;
                     if (count >= 50)
                     {
                        break;
                     }
                  }

                  if (size > 0)
                  {
                     data = new byte[size];
                     Array.Copy(buffer, data, size);
                     OnDataReceived(data);
                  }

                  Thread.Sleep(1);

                  if (_TcpClient == null)
                  {
                     _TcpClient = new TcpClient();
                     _TcpClient.Connect(new IPEndPoint(destinationAddress, destinationPort));
                     OnConnected();
                     BeginRead();
                  }
               }
               catch (Exception)
               {
                  try
                  {
                     if (_TcpClient != null)
                        _TcpClient.Close();
                  }
                  catch (Exception) { }
                  _TcpClient = null;
                  OnDisconnected();
               }
            }
         }, TaskCreationOptions.LongRunning);

         _TaskOut = Task.Factory.StartNew(() =>
         {
            if (File.Exists("dump.txt"))
            {
               File.Delete("dump.txt");
            }
            byte[] buffer = new byte[maxRead * 50];

            while (_IsRunning)
            {
               byte[] data;

               int size = 0;
               int count = 0;

               while (_QueueOut.TryTake(out data, 10))
               {
                  Array.Copy(data, 0, buffer, size, data.Length);
                  size += data.Length;
                  count++;
                  if (count >= 50)
                  {
                     break;
                  }
               }

               if (size > 0)
               {
                  try
                  {
                     if (_TcpClient == null)
                     {
                        Thread.Sleep(200);
                     }
                     if (_TcpClient != null)
                     {
                        if (_TcpClient.Connected)
                        {
                           _TcpClient.GetStream().Write(buffer, 0, size);
                           if (DebugDump)
                           {
                              File.AppendAllText("dump.txt", string.Join(" ", buffer.Take(size).Select(b => string.Format("{0:X2} ", b))) + " ");
                           }
                        }
                     }
                     else
                     {
                        try
                        {
                           if (_TcpClient != null)
                              _TcpClient.Close();
                        }
                        catch (Exception) { }
                        _TcpClient = null;
                        OnDisconnected();
                     }
                  }
                  catch (Exception)
                  {
                     try
                     {
                        if (_TcpClient != null)
                           _TcpClient.Close();
                     }
                     catch (Exception) { }
                     _TcpClient = null;
                     OnDisconnected();
                  }
               }
               Thread.Sleep(1);
            }
         }, TaskCreationOptions.LongRunning);
      }

      public void BeginRead()
      {
         var ns = _TcpClient.GetStream();
         ns.BeginRead(data, 0, data.Length, EndRead, data);
      }

      public override void Dispose()
      {
         _IsRunning = false;
         if (_TcpClient != null)
         {
            _TcpClient.Close();
         }
         _TcpClient = null;
         _TaskIn.Wait();
         _TaskOut.Wait();
         base.Dispose();
      }

      public void EndRead(IAsyncResult result)
      {
         try
         {
            var buffer = (byte[])result.AsyncState;
            var ns = _TcpClient.GetStream();
            var bytesAvailable = ns.EndRead(result);

            byte[] data = new byte[bytesAvailable];
            Array.Copy(buffer, data, bytesAvailable);
            _QueueIn.Add(data);
            BeginRead();
         }
         catch (Exception)
         {
            try
            {
               if (_TcpClient != null)
                  _TcpClient.Close();
            }
            catch (Exception) { }
            _TcpClient = null;
            OnDisconnected();
         }
      }

      protected override void SendData(byte[] data)
      {
         _QueueOut.Add(data);
      }
   }
}