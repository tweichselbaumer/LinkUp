using System;

namespace LinkUp.Logic
{
    public abstract class LinkUpLabel : IDisposable
    {
        private ushort _ChildIdentifier = 0;
        private bool _IsInitialized = false;
        private DateTime _LastUpdate;
        private string _Name;
        private LinkUpSubNode _Owner;
        private ushort _ParentIdentifier = 0;

        internal ushort ChildIdentifier
        {
            get
            {
                return _ChildIdentifier;
            }

            set
            {
                _ChildIdentifier = value;
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

        internal bool IsInitialized
        {
            get
            {
                return _IsInitialized;
            }

            set
            {
                _IsInitialized = value;
            }
        }

        abstract internal LinkUpLabelType LabelType
        {
            get;
        }

        internal DateTime LastUpdate
        {
            get
            {
                return _LastUpdate;
            }

            set
            {
                _LastUpdate = value;
            }
        }

        internal LinkUpSubNode Owner
        {
            get
            {
                return _Owner;
            }

            set
            {
                _Owner = value;
            }
        }

        internal ushort ParentIdentifier
        {
            get
            {
                return _ParentIdentifier;
            }

            set
            {
                _ParentIdentifier = value;
            }
        }

        public abstract void Dispose();

        internal static LinkUpLabel CreateNew(LinkUpLabelType type)
        {
            switch (type)
            {
                case LinkUpLabelType.Boolean:
                    return new LinkUpPrimitiveLabel<bool>();

                case LinkUpLabelType.Byte:
                    return new LinkUpPrimitiveLabel<byte>();

                case LinkUpLabelType.Double:
                    return new LinkUpPrimitiveLabel<double>();

                case LinkUpLabelType.Int16:
                    return new LinkUpPrimitiveLabel<short>();

                case LinkUpLabelType.Int32:
                    return new LinkUpPrimitiveLabel<int>();

                case LinkUpLabelType.Int64:
                    return new LinkUpPrimitiveLabel<long>();

                case LinkUpLabelType.SByte:
                    return new LinkUpPrimitiveLabel<sbyte>();

                case LinkUpLabelType.Single:
                    return new LinkUpPrimitiveLabel<float>();

                case LinkUpLabelType.UInt16:
                    return new LinkUpPrimitiveLabel<ushort>();

                case LinkUpLabelType.UInt32:
                    return new LinkUpPrimitiveLabel<uint>();

                case LinkUpLabelType.UInt64:
                    return new LinkUpPrimitiveLabel<ulong>();

                default:
                    return null;
            }
        }
    }
}