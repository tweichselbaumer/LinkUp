using System;
using System.Linq;
using System.Text;

namespace LinkUp.Logic
{
    internal class LinkUpNameRequest : LinkUpLogic
    {
        private string _Name;
        private LinkUpLabelType _LabelType;

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

        protected override void ParseFromRaw(byte[] data)
        {
            LabelType = (LinkUpLabelType)data[1];
            Name = Encoding.UTF8.GetString(data, 2, data.Length - 2);
        }

        protected override byte[] ToRaw()
        {
            return new byte[] { (byte)LinkUpType.NameRequest, (byte)LabelType }.Concat(Encoding.UTF8.GetBytes(Name)).ToArray();
        }
    }
}