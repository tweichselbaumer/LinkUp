namespace LinkUp.Portable
{
    public delegate void ReveicedDataEventHandler(byte[] data);

    public delegate void ReveicedPacketEventHandler(LinkUpConnector connector, LinkUpPacket packet);

    public abstract class LinkUpConnector
    {
        private LinkUpConverter _Converter = new LinkUpConverter();
        private string _Name;

        public LinkUpConnector()
        {
            ReveivedData += LinkUpConnector_ReveivedData;
        }

        public event ReveicedPacketEventHandler ReveivedPacket;

        protected event ReveicedDataEventHandler ReveivedData;

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

        internal void SendPacket(LinkUpPacket packet)
        {
            SendData(_Converter.ConvertToSend(packet));
        }

        protected abstract void Dispose();

        protected abstract void SendData(byte[] data);

        private void LinkUpConnector_ReveivedData(byte[] data)
        {
            foreach (LinkUpPacket packet in _Converter.ConvertFromReceived(data))
            {
                ReveivedPacket?.Invoke(this, packet);
            }
        }
    }
}