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

using System.IO.Pipes;

namespace LinkUp.Cs.Raw
{
   public class LinkUpNamedPipeConnector : LinkUpConnector
   {
      private const int BUFFER_SIZE = 1024;

      private const int TIMEOUT = 100;

      private bool _IsRunning = true;

      private Mode _Mode;

      private string _Name;

      private PipeStream _Stream;

      private Task _Task;

      public LinkUpNamedPipeConnector(string name, Mode mode)
      {
         _Name = name;
         _Mode = mode;

         if (mode == Mode.Server)
         {
            _Task = Task.Factory.StartNew(() =>
            {
               _Stream = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
               (_Stream as NamedPipeServerStream).WaitForConnection();
               Task localReadTask = null;
               while (_IsRunning)
               {
                  try
                  {
                     byte[] dataIn;

                     localReadTask = Task.Run(() =>
                         {
                            try
                            {
                               while (_IsRunning)
                               {
                                  dataIn = new byte[BUFFER_SIZE];
                                  int bytesRead = _Stream.Read(dataIn, 0, BUFFER_SIZE);
                                  if (bytesRead > 0)
                                  {
                                     byte[] result = new byte[bytesRead];
                                     Array.Copy(dataIn, result, bytesRead);
                                     Task.Run(() => { OnDataReceived(result); });
                                  }
                               }
                            }
                            catch (Exception) { }
                         });

                     if (!(_Stream as NamedPipeServerStream).IsConnected)
                     {
                        (_Stream as NamedPipeServerStream).Close();
                        _Stream = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                        (_Stream as NamedPipeServerStream).WaitForConnection();
                     }
                     localReadTask.Wait();
                  }
                  catch (Exception)
                  {
                  }
               }
               _Stream.Close();
            }, TaskCreationOptions.LongRunning);
         }
         else if (mode == Mode.Client)
         {
            _Task = Task.Run(() =>
            {
               _Stream = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
               (_Stream as NamedPipeClientStream).Connect();
               Task localTask = null;
               while (_IsRunning)
               {
                  try
                  {
                     byte[] dataIn;
                     if (localTask == null || localTask.IsCanceled || localTask.IsCompleted || localTask.IsFaulted)
                     {
                        localTask = Task.Run(() =>
                            {
                               dataIn = new byte[BUFFER_SIZE];

                               int bytesRead = _Stream.Read(dataIn, 0, BUFFER_SIZE);
                               if (bytesRead > 0)
                               {
                                  byte[] result = new byte[bytesRead];
                                  Array.Copy(dataIn, result, bytesRead);
                                  OnDataReceived(result);
                               }
                            });
                     }
                  }
                  catch (Exception)
                  {
                  }
               }
               _Stream.Close();
            });
         }
      }

      public enum Mode
      {
         Server,
         Client
      }

      public override void Dispose()
      {
         _IsRunning = false;
         _Task.Wait();
         IsDisposed = true;
      }

      protected override void SendData(byte[] data)
      {
         _Stream.Write(data, 0, data.Length);
         _Stream.Flush();
      }
   }
}