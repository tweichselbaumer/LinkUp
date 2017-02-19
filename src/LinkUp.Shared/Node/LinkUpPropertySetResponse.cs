﻿using System;
using System.Linq;

namespace LinkUp.Node
{
    internal class LinkUpPropertySetResponse : LinkUpLogic
    {

        private ushort _Identifier;



        public ushort Identifier
        {
            get
            {
                return _Identifier;
            }

            set
            {
                _Identifier = value;
            }
        }

        protected override void ParseFromRaw(byte[] data)
        {
            Identifier = BitConverter.ToUInt16(data, 1);
        }

        protected override byte[] ToRaw()
        {
            return new byte[] { (byte)LinkUpLogicType.PropertySetResponse }.Concat(BitConverter.GetBytes(Identifier)).ToArray();
        }
    }
}