using System;

namespace LinkUp.Node
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

        internal static LinkUpLabel CreateNew(LinkUpLabelType type, byte[] options)
        {
            switch (type)
            {
                case LinkUpLabelType.Node:
                    return null;
                case LinkUpLabelType.Function:
                    return null;
                case LinkUpLabelType.Event:
                    return LinkUpEventLabel.CreateNew(options);
                case LinkUpLabelType.Property:
                    return LinkUpPropertyLabelBase.CreateNew(options);
                default:
                    return null;
            }
        }
    }
}