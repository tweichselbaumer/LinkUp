using System;

namespace LinkUp.Raw
{
    public delegate void ReveicedPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);
    public delegate void SentPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public abstract class LinkUpConnector : IDisposable
    {
        private LinkUpConverter _Converter = new LinkUpConverter();
        private string _Name;
        private bool _IsDisposed;

        public event ReveicedPacketEventHandler ReveivedPacket;
        public event SentPacketEventHandler SentPacket;

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

        public abstract void Dispose();

        public void SendPacket(LinkUpPacket packet)
        {
            SendData(_Converter.ConvertToSend(packet));
            SentPacket?.Invoke(this, packet);
        }

        protected void OnDataReceived(byte[] data)
        {
            foreach (LinkUpPacket packet in _Converter.ConvertFromReceived(data))
            {
                ReveivedPacket?.Invoke(this, packet);
            }
        }

        protected abstract void SendData(byte[] data);
    }
}