using System;

namespace LinkUp.Raw
{
    public delegate void ReveicedPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public delegate void SentPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public abstract class LinkUpConnector : IDisposable
    {
        private LinkUpConverter _Converter = new LinkUpConverter();
        private bool _IsDisposed;
        private string _Name;

        public event ReveicedPacketEventHandler ReveivedPacket;

        public event SentPacketEventHandler SentPacket;

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
                if (ReveivedPacket != null)
                {
                    var receivers = ReveivedPacket.GetInvocationList();
                    foreach (ReveicedPacketEventHandler receiver in receivers)
                    {
                        receiver.BeginInvoke(this, packet, null, null);
                    }
                }

                //ReveivedPacket?.Invoke(this, packet);
            }
        }

        protected abstract void SendData(byte[] data);
    }
}