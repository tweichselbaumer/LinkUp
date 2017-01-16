using System;

namespace LinkUp.Raw
{
    public delegate void ReveicedPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public abstract class LinkUpConnector : IDisposable
    {
        private LinkUpConverter _Converter = new LinkUpConverter();
        private string _Name;

        public event ReveicedPacketEventHandler ReveivedPacket;

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