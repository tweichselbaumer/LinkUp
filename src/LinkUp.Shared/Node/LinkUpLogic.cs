using LinkUp.Raw;

namespace LinkUp.Node
{
    abstract internal class LinkUpLogic
    {
        internal static LinkUpLogic ParseFromPacket(LinkUpPacket packet)
        {
            //TODO: implement checks
            LinkUpLogicType type = (LinkUpLogicType)packet.Data[0];
            LinkUpLogic logic = null;

            switch (type)
            {
                case LinkUpLogicType.NameRequest:
                    logic = new LinkUpNameRequest();
                    break;

                case LinkUpLogicType.NameResponse:
                    logic = new LinkUpNameResponse();
                    break;

                case LinkUpLogicType.PropertyGetRequest:
                    logic = new LinkUpPropertyGetRequest();
                    break;

                case LinkUpLogicType.PropertyGetResponse:
                    logic = new LinkUpPropertyGetResponse();
                    break;

                case LinkUpLogicType.PropertySetRequest:
                    logic = new LinkUpPropertySetRequest();
                    break;

                case LinkUpLogicType.PropertySetResponse:
                    logic = new LinkUpPropertySetResponse();
                    break;

                case LinkUpLogicType.PingRequest:
                    logic = new LinkUpPingRequest();
                    break;

                case LinkUpLogicType.PingResponse:
                    logic = new LinkUpPingResponse();
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