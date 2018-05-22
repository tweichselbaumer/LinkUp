using System;
using System.Linq;
using System.Text;

namespace LinkUp.Node
{
    internal class LinkUpNameRequest : LinkUpLogic
    {
        private LinkUpLabelType _LabelType;
        private string _Name;

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
            UInt16 stringLength = BitConverter.ToUInt16(data, 2);
            Name = Encoding.UTF8.GetString(data, 4, stringLength);
        }

        protected override byte[] ToRaw()
        {
            return new byte[] { (byte)LinkUpLogicType.NameRequest, (byte)LabelType }.Concat(BitConverter.GetBytes(((UInt16)Name.Length))).Concat(Encoding.UTF8.GetBytes(Name)).ToArray();
        }
    }
}