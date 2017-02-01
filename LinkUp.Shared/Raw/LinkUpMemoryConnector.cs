using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LinkUp.Raw
{
    public class LinkUpMemoryConnector : LinkUpConnector
    {
        private const int TIMEOUT = 100;
        private BlockingCollection<byte[]> _InStream;
        private BlockingCollection<byte[]> _OutStream;
        private Task _Task;
        private bool _IsRunning = true;

        public LinkUpMemoryConnector(BlockingCollection<byte[]> inStream, BlockingCollection<byte[]> outStream)
        {
            _InStream = inStream;
            _OutStream = outStream;
            _Task = Task.Factory.StartNew(() =>
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
        }

        protected override void SendData(byte[] data)
        {
            _OutStream.Add(data);
        }
    }
}
