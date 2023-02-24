using System;
using System.Linq;
using System.Text;

namespace LinkUp.Node.Logic
{
    internal class LinkUpNameRequest : LinkUpLogic
    {
        private LinkUpLabelType _LabelType;
        private string _Name;
        private byte[] _Options;

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

        public byte[] Options
        {
            get
            {
                return _Options;
            }

            set
            {
                _Options = value;
            }
        }

        protected override void ParseFromRaw(byte[] data)
        {
            LabelType = (LinkUpLabelType)data[1];
            UInt16 stringLength = BitConverter.ToUInt16(data, 2);
            Name = Encoding.UTF8.GetString(data, 4, stringLength);
            if (data.Length - 4 - stringLength > 0)
            {
                _Options = new byte[data.Length - 4 - stringLength];
                Array.Copy(data, 4 + stringLength, _Options, 0, data.Length - 4 - stringLength);
            }
            else
            {
                _Options = null;
            }
        }

        protected override byte[] ToRaw()
        {
            return new byte[] { (byte)LinkUpLogicType.NameRequest, (byte)LabelType }.Concat(BitConverter.GetBytes(((UInt16)Name.Length))).Concat(Encoding.UTF8.GetBytes(Name)).Concat(Options).ToArray();
        }
    }
}