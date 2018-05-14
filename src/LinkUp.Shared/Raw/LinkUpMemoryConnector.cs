using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LinkUp.Raw
{
    public class LinkUpMemoryConnector : LinkUpConnector
    {
        private const int TIMEOUT = 100;
        private BlockingCollection<byte[]> _InStream;
        private bool _IsRunning = true;
        private BlockingCollection<byte[]> _OutStream;
        private Task _Task;

        public LinkUpMemoryConnector(BlockingCollection<byte[]> inStream, BlockingCollection<byte[]> outStream)
        {
            _InStream = inStream;
            _OutStream = outStream;
            _Task = Task.Run(() =>
            {
                while (_IsRunning)
                {
                    byte[] data;
                    _InStream.TryTake(out data, TIMEOUT);
                    if (data != null)
                    {
                        OnDataReceived(data);
                    }
                }
            });
        }

        public override void Dispose()
        {
            if (_Task != null && _Task.Status == TaskStatus.Running)
            {
                _IsRunning = false;
                _Task.Wait();
            }
            IsDisposed = true;
        }

        protected override void SendData(byte[] data)
        {
            _OutStream.Add(data);
        }
    }
}