using System;
using System.Threading;

namespace LinkUp.Cs.Node
{
   public delegate void FunctionLabelEventHandler(LinkUpFunctionLabel label, byte[] data);

   public class LinkUpFunctionLabel : LinkUpLabel
   {
      private const int CALL_REQUEST_TIMEOUT = 10000;
      private AutoResetEvent _CallAutoResetEvent = new AutoResetEvent(false);
      private byte[] _TempData;

      public event FunctionLabelEventHandler Return;

      internal override LinkUpLabelType LabelType
      {
         get
         {
            return LinkUpLabelType.Function;
         }
      }

      public void AsyncCall(byte[] data)
      {
         if (Owner != null)
         {
            Owner.CallFunction(this, data);
         }
      }

      public byte[] Call(byte[] data)
      {
         if (Owner != null)
         {
            _CallAutoResetEvent.Reset();
            Owner.CallFunction(this, data);
            if (!_CallAutoResetEvent.WaitOne(CALL_REQUEST_TIMEOUT))
               throw new Exception(string.Format("Unable to call function: {0}.", Name));
            return _TempData;
         }
         return null;
      }

      public override void Dispose()
      {
      }

      internal static LinkUpFunctionLabel CreateNew(byte[] options)
      {
         return new LinkUpFunctionLabel();
      }

      internal void DoEvent(byte[] data)
      {
         _TempData = data;
         _CallAutoResetEvent.Set();
         if (Return != null)
         {
            var receivers = Return.GetInvocationList();
            foreach (FunctionLabelEventHandler receiver in receivers)
            {
               receiver.BeginInvoke(this, data, null, null);
            }
         }
      }
   }
}