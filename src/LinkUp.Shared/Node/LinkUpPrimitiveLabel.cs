using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LinkUp.Node
{
    public abstract class LinkUpPrimitiveBaseLabel : LinkUpLabel
    {
        internal abstract byte[] Data { get; set; }

        internal abstract void GetDone(byte[] data);

        internal abstract void SetDone();
    }

    public class LinkUpPrimitiveLabel<T> : LinkUpPrimitiveBaseLabel
#if NET45
where T : IConvertible, new()
#else
where T : new()
#endif
    {
        private const int GET_REQUEST_TIMEOUT = 1000;
        private const int SET_REQUEST_TIMEOUT = 1000;
        private AutoResetEvent _AutoResetEvent = new AutoResetEvent(false);
        private List<BlockingCollection<T>> _Consumer = new List<BlockingCollection<T>>();
        private volatile bool _GetRequestIsRunning;
        private volatile bool _SetRequestIsRunning;
        private T _Value;

        public T Value
        {
            get
            {
                T result;
#if DEBUG
                Stopwatch watch = new Stopwatch();
                watch.Start();
#endif
                if (Owner == null)
                    result = _Value;
                else
                    result = RequestValue();
#if DEBUG
                watch.Stop();
                Debug.WriteLine(string.Format("Get value '{0}' took {1}ms.", Name, watch.ElapsedTicks * 1000 / Stopwatch.Frequency));
#endif
                return result;
            }

            set
            {
#if DEBUG
                Stopwatch watch = new Stopwatch();
                watch.Start();
#endif
                if (Owner == null)
                    _Value = value;
                else
                    SetValue(value);
#if DEBUG
                watch.Stop();
                Debug.WriteLine(string.Format("Set value '{0}' took {1}ms.", Name, watch.ElapsedTicks * 1000 / Stopwatch.Frequency));
#endif
            }
        }

        internal override byte[] Data
        {
            set
            {
                Value = (T)ConvertFromBytes(value);
            }

            get
            {
                return ConvertToBytes(Value);
            }
        }

        internal override LinkUpLabelType LabelType
        {
            get
            {
                if (_Value is bool)
                {
                    return LinkUpLabelType.Boolean;
                }
                if (_Value is sbyte)
                {
                    return LinkUpLabelType.SByte;
                }
                if (_Value is byte)
                {
                    return LinkUpLabelType.Byte;
                }
                if (_Value is short)
                {
                    return LinkUpLabelType.Int16;
                }
                if (_Value is ushort)
                {
                    return LinkUpLabelType.UInt16;
                }
                if (_Value is int)
                {
                    return LinkUpLabelType.Int32;
                }
                if (_Value is uint)
                {
                    return LinkUpLabelType.UInt32;
                }
                if (_Value is long)
                {
                    return LinkUpLabelType.Int64;
                }
                if (_Value is ulong)
                {
                    return LinkUpLabelType.UInt64;
                }
                if (_Value is float)
                {
                    return LinkUpLabelType.Single;
                }
                if (_Value is double)
                {
                    return LinkUpLabelType.Double;
                }
                throw new Exception("Unknow type for LinkUpLabel.");
            }
        }

        public override void Dispose()
        {
            //TODO:Close all
        }

        internal override void GetDone(byte[] data)
        {
            List<BlockingCollection<T>> consumer;
            lock (_Consumer)
            {
                consumer = _Consumer.Select(c => c).ToList();
                _Consumer.Clear();
                _GetRequestIsRunning = false;
            }
            foreach (BlockingCollection<T> wait in consumer)
            {
                wait.Add((T)ConvertFromBytes(data));
            }
        }

        internal override void SetDone()
        {
            _AutoResetEvent.Set();
        }

        private object ConvertFromBytes(byte[] value)
        {
            if (_Value is bool)
            {
                return BitConverter.ToBoolean(value, 0);
            }
            if (_Value is sbyte)
            {
                return (sbyte)value[0];
            }
            if (_Value is byte)
            {
                return value[0];
            }
            if (_Value is short)
            {
                return BitConverter.ToInt16(value, 0);
            }
            if (_Value is ushort)
            {
                return BitConverter.ToUInt16(value, 0);
            }
            if (_Value is int)
            {
                return BitConverter.ToInt32(value, 0);
            }
            if (_Value is uint)
            {
                return BitConverter.ToUInt32(value, 0);
            }
            if (_Value is long)
            {
                return BitConverter.ToInt64(value, 0);
            }
            if (_Value is ulong)
            {
                return BitConverter.ToUInt64(value, 0);
            }
            if (_Value is float)
            {
                return BitConverter.ToSingle(value, 0);
            }
            if (_Value is double)
            {
                return BitConverter.ToDouble(value, 0);
            }
            throw new Exception("Unknow type for LinkUpLabel.");
        }

        private byte[] ConvertToBytes(object value)
        {
            if (_Value is bool)
            {
                return BitConverter.GetBytes((bool)value);
            }
            if (_Value is sbyte)
            {
                return BitConverter.GetBytes((sbyte)value);
            }
            if (_Value is byte)
            {
                return BitConverter.GetBytes((byte)value);
            }
            if (_Value is short)
            {
                return BitConverter.GetBytes((short)value);
            }
            if (_Value is ushort)
            {
                return BitConverter.GetBytes((ushort)value);
            }
            if (_Value is int)
            {
                return BitConverter.GetBytes((int)value);
            }
            if (_Value is uint)
            {
                return BitConverter.GetBytes((uint)value);
            }
            if (_Value is long)
            {
                return BitConverter.GetBytes((long)value);
            }
            if (_Value is ulong)
            {
                return BitConverter.GetBytes((ulong)value);
            }
            if (_Value is float)
            {
                return BitConverter.GetBytes((float)value);
            }
            if (_Value is double)
            {
                return BitConverter.GetBytes((double)value);
            }
            throw new Exception("Unknow type for LinkUpLabel.");
        }

        private T RequestValue()
        {
            BlockingCollection<T> _Wait = new BlockingCollection<T>();
            lock (_Consumer)
            {
                _Consumer.Add(_Wait);
            }
            if (!_GetRequestIsRunning)
            {
                _GetRequestIsRunning = true;

                T value = new T();
                Task task = Task.Run(() =>
                {
                    do
                    {
                        Owner.GetLabel(this);
                    } while (!_Wait.TryTake(out value, GET_REQUEST_TIMEOUT));
                });
                task.Wait();
                _GetRequestIsRunning = false;
                return value;
            }
            else
            {
                return _Wait.Take();
            }
        }

        private void SetValue(T value)
        {
            if (!_SetRequestIsRunning)
            {
                _SetRequestIsRunning = true;

                Task task = Task.Run(() =>
                {
                    _AutoResetEvent.Reset();
                    do
                    {
                        Owner.SetLabel(this, ConvertToBytes(value));
                    } while (!_AutoResetEvent.WaitOne(SET_REQUEST_TIMEOUT));
                });
                _SetRequestIsRunning = false;
                task.Wait();
            }
            else
            {
                _AutoResetEvent.WaitOne();
            }
        }
    }
}