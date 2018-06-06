using System.Collections.Generic;
using System.Linq;

namespace LinkUp.Raw
{
    internal class LinkUpBytesPerSecondCounter
    {
        private List<long> _List = new List<long>();
        private long current;

#if NET45 || NETCOREAPP2_0
        private System.Timers.Timer _Timer;
#endif

        internal LinkUpBytesPerSecondCounter()
        {
            _List.AddRange(new long[5].ToList());

#if NET45 || NETCOREAPP2_0
            _Timer = new System.Timers.Timer(1000);
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
#endif
        }

#if NET45 || NETCOREAPP2_0

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_List)
            {
                _List.Add(current);
                current = 0;
                _List.RemoveAt(0);
            }
        }
#endif

        internal double BytesPerSecond
        {
            get
            {
                double result;
                lock (_List)
                {
                    result = (_List.Sum() + current) / (_List.Count + 1);
                }
                return result;
            }
        }

        internal void AddBytes(long bytes)
        {
            current += bytes;
        }
    }
}