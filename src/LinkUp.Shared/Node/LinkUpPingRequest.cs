using System;
using System.Collections.Generic;
using System.Text;

namespace LinkUp.Node
{
    internal class LinkUpPingRequest : LinkUpLogic
    {
        protected override void ParseFromRaw(byte[] data)
        {
        }

        protected override byte[] ToRaw()
        {
            return new byte[] { (byte)LinkUpLogicType.PingRequest };
        }
    }
}
