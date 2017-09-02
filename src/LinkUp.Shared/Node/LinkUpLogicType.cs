namespace LinkUp.Node
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
        PingResponse = 8
    }
}