﻿using System;
using System.Linq;
using System.Text;

namespace LinkUp.Logic
{
    public class LinkUpNameResponse : LinkUpLogic
    {
        private ushort _Identifier;
        private LinkUpLabelType _LabelType;
        private string _Name;

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

        public LinkUpLabelType LabelType
        {
            get
            {
                return _LabelType;
            }

            set
            {
                _LabelType = value;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }

            set
            {
                _Name = value;
            }
        }

        protected override void ParseFromRaw(byte[] data)
        {
            LabelType = (LinkUpLabelType)data[1];
            Identifier = BitConverter.ToUInt16(data, 2);
            string name = Encoding.UTF8.GetString(data.ToList().Skip(2).ToArray());
            Name = name.Substring(0, name.IndexOf('\0'));
        }

        protected override byte[] ToRaw()
        {
            return new byte[] { (byte)LinkUpType.NameRequest, (byte)LabelType }.Concat(BitConverter.GetBytes(Identifier)).Concat(Encoding.UTF8.GetBytes(Name)).ToArray();
        }
    }
}