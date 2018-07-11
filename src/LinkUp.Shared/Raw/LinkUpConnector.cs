using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUp.Raw
{
    public delegate void ConnectivityChangedEventHandler(LinkUpConnector connector, LinkUpConnectivityState connectivity);

    public delegate void ReveicedPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public delegate void SentPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public delegate void MetricUpdateEventHandler(LinkUpConnector connector, double bytesSentPerSecond, double bytesReceivedPerSecond);

    public enum LinkUpConnectivityState
    {
        Connected,
        Disconnected
    }

    public abstract class LinkUpConnector : IDisposable
    {
        private BlockingCollection<LinkUpPacket> _BlockingCollection = new BlockingCollection<LinkUpPacket>();
        private LinkUpConverter _Converter = new LinkUpConverter();
        private Task _Task;
        private bool _IsDisposed;
        private string _Name;
        private LinkUpBytesPerSecondCounter _ReceiveCounter = new LinkUpBytesPerSecondCounter();
        private LinkUpBytesPerSecondCounter _SentCounter = new LinkUpBytesPerSecondCounter();
        private long _TotalReceivedBytes;
        private long _TotalSentBytes;
        private int _TotalSentPackets;
        private bool _IsRunning;

#if NET45 || NETCOREAPP2_0
        private System.Timers.Timer _Timer;
#endif

        public LinkUpConnector()
        {
            _IsRunning = true;
            _Task = Task.Factory.StartNew(OnDataReceivedWorker, TaskCreationOptions.LongRunning);
#if NET45 || NETCOREAPP2_0
            _Timer = new System.Timers.Timer(1000);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
#endif
        }

        public event ConnectivityChangedEventHandler ConnectivityChanged;

        public event ReveicedPacketEventHandler ReveivedPacket;

        public event SentPacketEventHandler SentPacket;

        public event MetricUpdateEventHandler MetricUpdate;

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
            _IsRunning = false;
            _Task.Wait();
#if NET45 || NETCOREAPP2_0
            _Timer.Dispose();
#endif
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

        private void OnDataReceivedWorker()
        {
            while (_IsRunning)
            {
                LinkUpPacket packet = _BlockingCollection.Take();
                ReveivedPacket?.Invoke(this, packet);
            }
        }

        protected void OnConnected()
        {
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

#if NET45 || NETCOREAPP2_0

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MetricUpdate?.Invoke(this, SentBytesPerSecond, ReceivedBytesPerSecond);
        }

#endif
    }
}