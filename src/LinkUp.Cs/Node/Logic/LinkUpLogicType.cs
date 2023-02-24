namespace LinkUp.Node.Logic
{
    public enum LinkUpLogicType : byte
    {
        NameRequest = 1,
        NameResponse = 2,
        PropertyGetRequest = 3,
        PropertyGetResponse = 4,
        PropertySetRequest = 5,
        PropertySetResponse = 6,
        PingRequest = 7,
        PingResponse = 8,
        EventFireRequest = 9,
        EventFireResponse = 10,
        EventSubscribeRequest = 11,
        EventSubscribeResponse = 12,
        EventUnsubscribeRequest = 13,
        EventUnsubscribeResponse = 14,
        FunctionCallRequest = 15,
        FunctionCallResponse = 16
    }
}