using System;
using System.Collections.Generic;
using System.Text;

namespace LinkUp.Logic
{
    public enum LinkUpType : byte
    {
        NameRequest = 1,
        NameResponse = 2,
        PropertyGetRequest = 3,
        PropertyGetResponse = 4,
    }
}
