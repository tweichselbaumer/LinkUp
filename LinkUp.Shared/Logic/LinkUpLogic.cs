using LinkUp.Raw;
using System;
using System.Linq;

namespace LinkUp.Logic
{
    abstract public class LinkUpLogic
    {
        internal static LinkUpLogic ParseFromPacket(LinkUpPacket packet)
        {
            //TODO: implement checks
            LinkUpType type = (LinkUpType)packet.Data[0];
            LinkUpLogic logic = null;

            switch (type)
            {
                case LinkUpType.NameRequest:
                    logic = new LinkUpNameRequest();
                    break;
                case LinkUpType.NameResponse:
                    logic = new LinkUpNameResponse();
                    break;
            }

            logic?.ParseFromRaw(packet.Data);

            return logic;
        }

        internal LinkUpPacket ToPacket()
        {
            return new LinkUpPacket() { Data = ToRaw() };
        }

        protected abstract void ParseFromRaw(byte[] data);

        protected abstract byte[] ToRaw();
    }
}