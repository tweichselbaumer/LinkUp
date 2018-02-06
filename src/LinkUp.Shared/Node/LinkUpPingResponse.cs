using System;
using System.Collections.Generic;
using System.Text;

namespace LinkUp.Node
{
    internal class LinkUpPingResponse : LinkUpLogic
    {
        protected override void ParseFromRaw(byte[] data)
        {
        }

        protected override byte[] ToRaw()
        {
            return new byte[] { (byte)LinkUpLogicType.PingResponse };
        }
    }
}
