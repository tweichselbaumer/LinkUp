﻿using System;
using System.Linq;

namespace LinkUp.Node.Logic
{
    internal class LinkUpPropertySetRequest : LinkUpLogic
    {
        private byte[] _Data;
        private ushort _Identifier;

        public byte[] Data
        {
            get
            {
                return _Data;
            }

            set
            {
                _Data = value;
            }
        }

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
            _Data = new byte[data.Length - 3];
            Array.Copy(data, 3, _Data, 0, data.Length - 3);
        }

        protected override byte[] ToRaw()
        {
            return new byte[] { (byte)LinkUpLogicType.PropertySetRequest }.Concat(BitConverter.GetBytes(Identifier)).Concat(_Data).ToArray();
        }
    }
}