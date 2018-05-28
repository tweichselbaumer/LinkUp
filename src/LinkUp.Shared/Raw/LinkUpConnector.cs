using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUp.Raw
{
    public delegate void ReveicedPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public delegate void SentPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public delegate void ConnectivityChangedEventHandler(LinkUpConnector connector, LinkUpConnectivityType connectivity);

    public enum LinkUpConnectivityType
    {
        Connected,
        Disconnected
    }

    public abstract class LinkUpConnector : IDisposable
    {
        private LinkUpConverter _Converter = new LinkUpConverter();
        private bool _IsDisposed;
        private string _Name;
        private int _TotalSendPackets;
        private long _TotalSendBytes;
        private long _TotalReceivedBytes;

        public event ReveicedPacketEventHandler ReveivedPacket;

        public event SentPacketEventHandler SentPacket;

        public event ConnectivityChangedEventHandler ConnectivityChanged;

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

        public int TotalSendPackets
        {
            get
            {
                return _TotalSendPackets;
            }
        }

        public int TotalFailedPackets
        {
            get
            {
                return _Converter.TotalFailedPackets;
            }

        }

        public long TotalSendBytes
        {
            get
            {
                return _TotalSendBytes;
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

        public abstract void Dispose();

        public void SendPacket(LinkUpPacket packet)
        {
            byte[] data = _Converter.ConvertToSend(packet);
            SendData(data);
            SentPacket?.Invoke(this, packet);
            _TotalSendPackets++;
            _TotalSendBytes += data.Length;
            //Console.WriteLine("PTotal Received Packets: {0} Total Failed Packets: {1} Total Send Packets: {2} Bytes In: {3} Bytes Out: {4}", TotalReceivedPackets, TotalFailedPackets, TotalSendPackets, TotalReceivedBytes, TotalSendBytes);
        }

        protected void OnConnected()
        {
            if (ConnectivityChanged != null)
            {
                var receivers = ConnectivityChanged.GetInvocationList();
                foreach (ConnectivityChangedEventHandler receiver in receivers)
                {
                    receiver.BeginInvoke(this, LinkUpConnectivityType.Connected, null, null);
                }
            }
        }

        protected void OnDisconnected()
        {
            if (ConnectivityChanged != null)
            {
                var receivers = ConnectivityChanged.GetInvocationList();
                foreach (ConnectivityChangedEventHandler receiver in receivers)
                {
                    receiver.BeginInvoke(this, LinkUpConnectivityType.Disconnected, null, null);
                }
            }
        }

        protected void OnDataReceived(byte[] data)
        {
            _TotalReceivedBytes += data.Length;
            List<LinkUpPacket> list = _Converter.ConvertFromReceived(data);
            //Console.WriteLine(list.Count);
            foreach (LinkUpPacket packet in list)
            {
                if (ReveivedPacket != null)
                {
                    var receivers = ReveivedPacket.GetInvocationList();
                    foreach (ReveicedPacketEventHandler receiver in receivers)
                    {
                        receiver.BeginInvoke(this, packet, null, null);
                    }
                }
            }
        }

        protected abstract void SendData(byte[] data);
    }
}