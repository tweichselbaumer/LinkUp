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

                case LinkUpLogicType.EventFireRequest:
                    logic = new LinkUpEventFireRequest();
                    break;

                case LinkUpLogicType.EventFireResponse:
                    logic = new LinkUpEventFireResponse();
                    break;

                case LinkUpLogicType.EventSubscribeRequest:
                    logic = new LinkUpEventSubscribeRequest();
                    break;

                case LinkUpLogicType.EventSubscribeResponse:
                    logic = new LinkUpEventSubscribeResponse();
                    break;

                case LinkUpLogicType.EventUnsubscribeRequest:
                    logic = new LinkUpEventUnsubscribeRequest();
                    break;

                case LinkUpLogicType.EventUnsubscribeResponse:
                    logic = new LinkUpEventUnsubscribeResponse();
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