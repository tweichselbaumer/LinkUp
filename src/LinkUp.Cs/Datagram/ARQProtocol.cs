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

using NLog;
using System.Collections.Concurrent;

namespace LinkUp.Cs.Datagram
{
   public class ARQProtocol : DatagramProtocolDecorator
   {
      private const byte c_MaxResends = 3;
      private const UInt32 c_ResendTimeout = 5000;
      private const UInt32 c_WindowsSize = 5;

      private BlockingCollection<Int64> _ReceiveAck = new BlockingCollection<Int64>();
      private BlockingCollection<Datagram> _ReceiveQueue = new BlockingCollection<Datagram>();
      private UInt32 _ReceiveSessionId = 0;
      private Task _ReceiveTask;
      private ReceiveDatagramContainer?[] _ReceiveWindowBuffer = new ReceiveDatagramContainer?[c_WindowsSize];
      private BlockingCollection<Int64> _SendAck = new BlockingCollection<Int64>();
      private BlockingCollection<Datagram> _SendQueue = new BlockingCollection<Datagram>();
      private UInt32 _SendSessionId = 0;
      private Task _SendTask;
      private LinkedList<SendDatagramContainer> _SendWindowBuffer = new LinkedList<SendDatagramContainer>();

      public ARQProtocol(IDatagramProtocol nextLayer) : base(nextLayer)
      {
         _SendTask = Task.Factory.StartNew(OnDataReceivedWorker, TaskCreationOptions.LongRunning);
         _ReceiveTask = Task.Factory.StartNew(OnSendWorker, TaskCreationOptions.LongRunning);
      }

      private enum DatagramType : byte
      {
         Data = 0,
         Ack = 1,
         Reset = 2,
      };

      private void OnDataReceivedWorker()
      {
         Logger logger = LogManager.GetCurrentClassLogger();

         while (true)
         {
            Datagram? datagram = null;

            if (_ReceiveQueue.TryTake(out datagram, 10))
            {
               Header header = Header.FromBytes(datagram.ReadFront(Header.Size));
               datagram.DeleteFront(Header.Size);

               switch (header.Type)
               {
                  case DatagramType.Data:
                     {
                        DataHeader dataHeader = DataHeader.FromBytes(datagram.ReadFront(DataHeader.Size));
                        datagram.DeleteFront(DataHeader.Size);

                        logger.Debug("Recieved Packet with session id: {0} and size {1}", dataHeader.SessionId, datagram.Size());

                        if (_ReceiveSessionId > dataHeader.SessionId || dataHeader.SessionId >= _ReceiveSessionId + c_WindowsSize)
                        {
                           logger.Warn("Received Packet with session id {0} ({1} - {2})", dataHeader.SessionId, _ReceiveSessionId, _ReceiveSessionId + c_WindowsSize);

                           for (int i = 0; i < c_WindowsSize; i++)
                           {
                              _ReceiveWindowBuffer[i] = null;
                           }

                           _ReceiveSessionId = 0;

                           _SendAck.Add(-1);
                        }
                        else
                        {
                           _SendAck.Add(dataHeader.SessionId);

                           ReceiveDatagramContainer container = new ReceiveDatagramContainer();
                           container.Datagram = datagram;

                           _ReceiveWindowBuffer[dataHeader.SessionId - _ReceiveSessionId] = container;
                        }

                        while (_ReceiveWindowBuffer[0] != null && _ReceiveWindowBuffer[0].Datagram != null)
                        {
                           OnReceived(_ReceiveWindowBuffer[0].Datagram);

                           for (int i = 0; i < c_WindowsSize - 1; i++)
                           {
                              _ReceiveWindowBuffer[i] = _ReceiveWindowBuffer[i + 1];
                           }

                           _ReceiveWindowBuffer[c_WindowsSize - 1] = null;
                           _ReceiveSessionId++;
                        }
                     }
                     break;

                  case DatagramType.Ack:
                     {
                        AckHeader ackHeader = AckHeader.FromBytes(datagram.ReadFront(AckHeader.Size));
                        datagram.DeleteFront(AckHeader.Size);

                        logger.Debug("Recieved ACK with session id: {0}", ackHeader.SessionId);

                        _ReceiveAck.Add(ackHeader.SessionId);
                     }
                     break;

                  case DatagramType.Reset:
                     {
                        logger.Debug("Recieved RESET");

                        for (int i = 0; i < c_WindowsSize; i++)
                        {
                           _ReceiveWindowBuffer[i] = null;
                        }

                        _ReceiveSessionId = 0;

                        _ReceiveAck.Add(-1);
                     }
                     break;

                  default:
                     throw new InvalidOperationException();
               }
            }
         }
      }

      private void OnSendWorker()
      {
         Logger logger = LogManager.GetCurrentClassLogger();

         while (true)
         {
            if (_SendWindowBuffer.Count < c_WindowsSize)
            {
               Datagram? datagram = null;

               if (_SendQueue.TryTake(out datagram, 10))
               {
                  SendDatagramContainer container = new SendDatagramContainer();
                  container.IsAcknowlaged = false;
                  container.LastSendTime = 0;
                  container.Resends = 0;
                  container.SessionId = _SendSessionId;
                  container.Datagram = datagram;

                  _SendSessionId++;

                  logger.Debug("Add Packet with session id: {0}", container.SessionId);

                  Header header = new Header();
                  header.Type = DatagramType.Data;

                  DataHeader dataHeader = new DataHeader();
                  dataHeader.SessionId = container.SessionId;

                  container.Datagram.WriteFront(dataHeader.ToBytes());
                  container.Datagram.WriteFront(header.ToBytes());

                  _SendWindowBuffer.AddLast(container);
               }
            }

            {
               Int64 sessionId = 0;
               if (_ReceiveAck.TryTake(out sessionId, 10))
               {
                  if (sessionId == -1)
                  {
                     _SendSessionId = 0;

                     while (_SendWindowBuffer.Count > 0)
                     {
                        _SendWindowBuffer.RemoveFirst();
                     }
                  }
                  else
                  {
                     foreach (SendDatagramContainer container in _SendWindowBuffer)
                     {
                        if (container.SessionId == sessionId)
                        {
                           container.IsAcknowlaged = true;
                           break;
                        }
                     }
                  }
               }
            }

            {
               Int64 sessionId = 0;
               if (_SendAck.TryTake(out sessionId, 10))
               {
                  Datagram datagram = new Datagram();
                  Header header = new Header();

                  if (sessionId == -1)
                  {
                     header.Type = DatagramType.Reset;
                     _SendSessionId = 0;

                     while (_SendWindowBuffer.Count > 0)
                     {
                        _SendWindowBuffer.RemoveFirst();
                     }
                  }
                  else
                  {
                     header.Type = DatagramType.Ack;

                     AckHeader ackHeader = new AckHeader();
                     ackHeader.SessionId = (UInt32)sessionId;

                     datagram.WriteFront(ackHeader.ToBytes());
                  }

                  datagram.WriteFront(header.ToBytes());
                  NextLayer.Send(datagram);
               }
            }

            foreach (SendDatagramContainer container in _SendWindowBuffer)
            {
               if (!container.IsAcknowlaged && container.Resends < c_MaxResends && (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - container.LastSendTime > c_ResendTimeout)
               {
                  logger.Debug("Send Packet with session id: {0}, retry: {1} and size {2}", container.SessionId, container.Resends, container.Datagram.Size());
                  container.Resends++;
                  container.LastSendTime = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                  NextLayer.Send(container.Datagram);
               }
            }

            if (_SendWindowBuffer.Count > 0)
            {
               bool sendError = _SendWindowBuffer.First().Resends >= c_MaxResends &&
                 (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - _SendWindowBuffer.First().LastSendTime > c_ResendTimeout;
               if (_SendWindowBuffer.First().IsAcknowlaged || sendError)
               {
                  if (sendError)
                  {
                     logger.Warn("Unable to send Packet with session id: {0}", _SendWindowBuffer.First().SessionId);
                  }

                  _SendWindowBuffer.RemoveFirst();
               }
            }
         }
      }

      public override void ProgressReceived(Datagram datagram)
      {
         _ReceiveQueue.Add(datagram);
      }

      public override bool Send(Datagram datagram)
      {
         _SendQueue.Add(datagram);
         return true;
      }

      private class AckHeader
      {
         public static readonly int Size = 4;
         public UInt32 SessionId { get; set; } = 0;

         public static AckHeader FromBytes(byte[] data)
         {
            AckHeader header = new AckHeader();
            header.SessionId = BitConverter.ToUInt32(data, 0);
            return header;
         }

         public byte[] ToBytes()
         {
            return BitConverter.GetBytes(SessionId);
         }
      };

      private class DataHeader
      {
         public static readonly int Size = 4;
         public UInt32 SessionId { get; set; } = 0;

         public static DataHeader FromBytes(byte[] data)
         {
            DataHeader header = new DataHeader();
            header.SessionId = BitConverter.ToUInt32(data, 0);
            return header;
         }

         public byte[] ToBytes()
         {
            return BitConverter.GetBytes(SessionId);
         }
      };

      private class Header
      {
         public static readonly int Size = 1;

         public DatagramType Type { get; set; } = DatagramType.Data;

         public static Header FromBytes(byte[] data)
         {
            Header header = new Header();
            header.Type = (DatagramType)data[0];
            return header;
         }

         public byte[] ToBytes()
         {
            return new byte[1] { (byte)Type };
         }
      }

      private class ReceiveDatagramContainer
      {
         public Datagram? Datagram { get; set; } = null;
      };

      private class SendDatagramContainer
      {
         public Datagram? Datagram { get; set; } = null;
         public bool IsAcknowlaged { get; set; } = false;
         public long LastSendTime { get; set; } = 0;
         public byte Resends { get; set; } = 0;
         public UInt32 SessionId { get; set; } = 0;
      };
   }
}