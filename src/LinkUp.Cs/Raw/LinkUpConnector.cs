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

namespace LinkUp.Raw
{
   public delegate void ConnectivityChangedEventHandler(LinkUpConnector connector, LinkUpConnectivityState connectivity);

   public delegate void MetricUpdateEventHandler(LinkUpConnector connector, double bytesSentPerSecond, double bytesReceivedPerSecond);

   public delegate void ReveicedPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

   public delegate void SentPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

   public enum LinkUpConnectivityState
   {
      Connected,
      Disconnected
   }

   public abstract class LinkUpConnector : IDisposable
   {
      private BlockingCollection<LinkUpPacket> _BlockingCollection = new BlockingCollection<LinkUpPacket>();
      private CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();
      private LinkUpConnectivityState _ConnectivityState = LinkUpConnectivityState.Disconnected;
      private LinkUpConverter _Converter = new LinkUpConverter();
      private bool _DebugDump;
      private bool _IsDisposed;
      private bool _IsRunning;
      private string _Name;
      private LinkUpBytesPerSecondCounter _ReceiveCounter = new LinkUpBytesPerSecondCounter();
      private LinkUpBytesPerSecondCounter _SentCounter = new LinkUpBytesPerSecondCounter();
      private Task _Task;
      private System.Timers.Timer _Timer;
      private long _TotalReceivedBytes;
      private long _TotalSentBytes;
      private int _TotalSentPackets;

      public LinkUpConnector()
      {
         _IsRunning = true;
         _Task = Task.Factory.StartNew(OnDataReceivedWorker, TaskCreationOptions.LongRunning);

         _Timer = new System.Timers.Timer(1000);
         _Timer.Elapsed += _Timer_Elapsed;
         _Timer.Start();
      }

      public event ConnectivityChangedEventHandler ConnectivityChanged;

      public event MetricUpdateEventHandler MetricUpdate;

      public event ReveicedPacketEventHandler ReveivedPacket;

      public event SentPacketEventHandler SentPacket;

      public LinkUpConnectivityState ConnectivityState
      {
         get
         {
            return _ConnectivityState;
         }
      }

      public bool DebugDump
      {
         get
         {
            return _DebugDump;
         }

         set
         {
            _DebugDump = value;
         }
      }

      public bool IsDisposed
      {
         get
         {
            return _IsDisposed;
         }
         protected set
         {
            _IsDisposed = value;
         }
      }

      public string Name
      {
         get
         {
            return _Name;
         }

         set
         {
            _Name = value;
         }
      }

      public double ReceivedBytesPerSecond
      {
         get
         {
            return _ReceiveCounter.BytesPerSecond;
         }
      }

      public double SentBytesPerSecond
      {
         get
         {
            return _SentCounter.BytesPerSecond;
         }
      }

      public int TotalFailedPackets
      {
         get
         {
            return _Converter.TotalFailedPackets;
         }
      }

      public long TotalReceivedBytes
      {
         get
         {
            return _TotalReceivedBytes;
         }
      }

      public int TotalReceivedPackets
      {
         get
         {
            return _Converter.TotalReceivedPackets;
         }
      }

      public int TotalSendPackets
      {
         get
         {
            return _TotalSentPackets;
         }
      }

      public long TotalSentBytes
      {
         get
         {
            return _TotalSentBytes;
         }
      }

      public virtual void Dispose()
      {
         _ConnectivityState = LinkUpConnectivityState.Disconnected;
         _IsRunning = false;
         _CancellationTokenSource.Cancel();
         _Task.Wait();

         _Timer.Dispose();

         _CancellationTokenSource.Dispose();
      }

      public void SendPacket(LinkUpPacket packet)
      {
         byte[] data = _Converter.ConvertToSend(packet);
         SendData(data);
         SentPacket?.Invoke(this, packet);
         _TotalSentPackets++;
         _TotalSentBytes += data.Length;
         _SentCounter.AddBytes(data.Length);
      }

      protected void OnConnected()
      {
         _ConnectivityState = LinkUpConnectivityState.Connected;
         if (ConnectivityChanged != null)
         {
            var receivers = ConnectivityChanged.GetInvocationList();
            foreach (ConnectivityChangedEventHandler receiver in receivers)
            {
               receiver.BeginInvoke(this, LinkUpConnectivityState.Connected, null, null);
            }
         }
      }

      protected void OnDataReceived(byte[] data)
      {
         _TotalReceivedBytes += data.Length;
         _ReceiveCounter.AddBytes(data.Length);
         List<LinkUpPacket> list = _Converter.ConvertFromReceived(data);
         foreach (LinkUpPacket packet in list)
         {
            _BlockingCollection.Add(packet);
         }
      }

      protected void OnDisconnected()
      {
         _ConnectivityState = LinkUpConnectivityState.Disconnected;
         if (ConnectivityChanged != null)
         {
            var receivers = ConnectivityChanged.GetInvocationList();
            foreach (ConnectivityChangedEventHandler receiver in receivers)
            {
               receiver.BeginInvoke(this, LinkUpConnectivityState.Disconnected, null, null);
            }
         }
      }

      protected abstract void SendData(byte[] data);

      private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
      {
         MetricUpdate?.Invoke(this, SentBytesPerSecond, ReceivedBytesPerSecond);
      }

      private void OnDataReceivedWorker()
      {
         while (_IsRunning)
         {
            try
            {
               LinkUpPacket packet = _BlockingCollection.Take(_CancellationTokenSource.Token);
               ReveivedPacket?.Invoke(this, packet);
            }
            catch (Exception) { }
         }
      }
   }
}