using System;
using System.Threading;

namespace LinkUp.Node
{
    public delegate void FireEventLabelEventHandler(LinkUpEventLabel label, byte[] data);

    public class LinkUpEventLabel : LinkUpLabel
    {
        private const int SUBSCRIBE_REQUEST_TIMEOUT = 4000;
        private const int UNSUBSCRIBE_REQUEST_TIMEOUT = 4000;
        private bool _IsSubscribed;
        private AutoResetEvent _SubscribeAutoResetEvent = new AutoResetEvent(false);
        private AutoResetEvent _UnsubscribeAutoResetEvent = new AutoResetEvent(false);

        public event FireEventLabelEventHandler Fired;

        public bool IsSubscribed
        {
            get
            {
                return _IsSubscribed;
            }
        }

        internal override LinkUpLabelType LabelType
        {
            get
            {
                return LinkUpLabelType.Event;
            }
        }

        public static LinkUpEventLabel CreateNew(byte[] options)
        {
            return new LinkUpEventLabel();
        }

        public override void Dispose()
        {
        }

        public void Subscribe()
        {
            if (Owner != null)
            {
                _SubscribeAutoResetEvent.Reset();
                Owner.SubscribeEvent(this);
                if (!_SubscribeAutoResetEvent.WaitOne(SUBSCRIBE_REQUEST_TIMEOUT))
                    throw new Exception(string.Format("Unable to subscribe event: {0}.", Name));
            }
            _IsSubscribed = true;
        }

        public void Unsubscribe()
        {
            if (Owner != null)
            {
                _UnsubscribeAutoResetEvent.Reset();
                Owner.UnsubscribeEvent(this);
                if (!_UnsubscribeAutoResetEvent.WaitOne(UNSUBSCRIBE_REQUEST_TIMEOUT))
                    throw new Exception(string.Format("Unable to unsubscribe event: {0}.", Name));
            }
            _IsSubscribed = false;
        }

        internal void DoEvent(byte[] data)
        {
            if (Fired != null)
            {
                //var receivers = Fired.GetInvocationList();
                //foreach (FireEventLabelEventHandler receiver in receivers)
                //{
                //    receiver.BeginInvoke(this, data, null, null);
                //}
                Fired(this, data);
            }
        }

        internal void Resubscribe()
        {
            if (IsSubscribed)
            {
                Subscribe();
            }
            else
            {
                Unsubscribe();
            }
        }

        internal void SubscribeDone()
        {
            _SubscribeAutoResetEvent.Set();
        }

        internal void UnsubscribeDone()
        {
            _UnsubscribeAutoResetEvent.Set();
        }
    }
}