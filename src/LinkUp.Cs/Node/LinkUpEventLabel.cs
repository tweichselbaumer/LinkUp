using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Cs.Node
{
   public delegate void FireEventLabelEventHandler(LinkUpEventLabel label, byte[] data);

   public class LinkUpEventLabel : LinkUpLabel
   {
      private const int SUBSCRIBE_REQUEST_TIMEOUT = 4000;
      private const int UNSUBSCRIBE_REQUEST_TIMEOUT = 4000;
      private bool _IsSubscribed;
      private AutoResetEvent _SubscribeAutoResetEvent = new AutoResetEvent(false);
      private AutoResetEvent _UnsubscribeAutoResetEvent = new AutoResetEvent(false);

      private BlockingCollection<byte[]> _BlockingCollection = new BlockingCollection<byte[]>();

      private bool _IsRunning;
      private Task _Task;
      private CancellationTokenSource _CancellationTokenSource = new CancellationTokenSource();

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

      public LinkUpEventLabel()
      {
         _IsRunning = true;
         _Task = Task.Factory.StartNew(OnDataReceivedWorker, TaskCreationOptions.LongRunning);
      }

      public static LinkUpEventLabel CreateNew(byte[] options)
      {
         return new LinkUpEventLabel();
      }

      private void OnDataReceivedWorker()
      {
         while (_IsRunning)
         {
            try
            {
               byte[] data = _BlockingCollection.Take(_CancellationTokenSource.Token);
               Fired?.Invoke(this, data);
            }
            catch (Exception) { }
         }
      }

      public override void Dispose()
      {
         _IsRunning = false;
         _CancellationTokenSource.Cancel();
         _Task.Wait();
         _CancellationTokenSource.Dispose();
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
         _BlockingCollection.Add(data);
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