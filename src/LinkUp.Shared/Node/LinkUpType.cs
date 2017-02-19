namespace LinkUp.Node
{
    public enum LinkUpType : byte
    {
        NameRequest = 1,
        NameResponse = 2,
        PropertyGetRequest = 3,
        PropertyGetResponse = 4,
        PropertySetRequest = 5,
        PropertySetResponse = 6,
    }
}