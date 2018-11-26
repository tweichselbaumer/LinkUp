using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUp.Raw
{
    internal class LinkUpBytesPerSecondCounter : IDisposable
    {
        private Queue<long> _Queue = new Queue<long>();
        private long current;

#if NET45 || NETCOREAPP2_0
        private System.Timers.Timer _Timer;
#endif

        internal LinkUpBytesPerSecondCounter()
        {
            for (int i = 0; i < 5; i++)
            {
                _Queue.Enqueue(0);
            }

#if NET45 || NETCOREAPP2_0
            _Timer = new System.Timers.Timer(1000);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
#endif
        }

        internal double BytesPerSecond
        {
            get
            {
                double result;
                lock (_Queue)
                {
                    result = _Queue.Sum() / _Queue.Count;
                }
                return result;
            }
        }

        public void Dispose()
        {
#if NET45 || NETCOREAPP2_0

            if (_Timer != null)
            {
                _Timer.Dispose();
            }
#endif
        }

        internal void AddBytes(long bytes)
        {
            lock (_Queue)
            {
                current += bytes;
            }
        }

#if NET45 || NETCOREAPP2_0
        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_Queue)
            {
                _Queue.Enqueue(current);
                current = 0;
                _Queue.Dequeue();
            }
        }
#endif
    }
}