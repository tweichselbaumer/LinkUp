namespace LinkUp.Node
{
    public delegate void FunctionLabelEventHandler(LinkUpFunctionLabel label, byte[] data);

    public class LinkUpFunctionLabel : LinkUpLabel
    {
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

        public override void Dispose()
        {
        }

        internal static LinkUpFunctionLabel CreateNew(byte[] options)
        {
            return new LinkUpFunctionLabel();
        }

        internal void DoEvent(byte[] data)
        {
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