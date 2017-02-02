using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkUp.Logic
{
    public abstract class LinkUpPrimitiveBaseLabel : LinkUpLabel
    {
        internal abstract byte[] Data { get; set; }
    }

    public class LinkUpPrimitiveLabel<T> : LinkUpPrimitiveBaseLabel
#if NET45
where T : IConvertible, new()
#else
where T : new()
#endif
    {
        private List<BlockingCollection<T>> _Consumer = new List<BlockingCollection<T>>();
        private bool _RequestIsRunning;
        private T _Value;

        public T Value
        {
            get
            {
                if (Owner == null)
                    return _Value;
                else
                    return RequestValue();
            }

            set
            {
                if (Owner == null)
                    _Value = value;
                else
                    SetValue(value);
            }
        }

        internal override byte[] Data
        {
            set
            {
                List<BlockingCollection<T>> consumer;
                lock (_Consumer)
                {
                    consumer = _Consumer.Select(c => c).ToList();
                    _Consumer.Clear();
                    _RequestIsRunning = false;
                }
                foreach (BlockingCollection<T> wait in consumer)
                {
                    wait.Add((T)ConvertFromBytes(value));
                }
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
            //if (_Value is bool)
            //{
            //    return BitConverter.ToBoolean(value, 0);
            //}
            //if (_Value is sbyte)
            //{
            //    return (sbyte)value[0];
            //}
            //if (_Value is byte)
            //{
            //    return value[0];
            //}
            //if (_Value is short)
            //{
            //    return BitConverter.ToInt16(value, 0);
            //}
            //if (_Value is ushort)
            //{
            //    return BitConverter.ToUInt16(value, 0);
            //}
            if (_Value is int)
            {
                return BitConverter.GetBytes((int)value);
            }
            //if (_Value is uint)
            //{
            //    return BitConverter.ToUInt32(value, 0);
            //}
            //if (_Value is long)
            //{
            //    return BitConverter.ToInt64(value, 0);
            //}
            //if (_Value is ulong)
            //{
            //    return BitConverter.ToUInt64(value, 0);
            //}
            //if (_Value is float)
            //{
            //    return BitConverter.ToSingle(value, 0);
            //}
            //if (_Value is double)
            //{
            //    return BitConverter.ToDouble(value, 0);
            //}
            throw new Exception("Unknow type for LinkUpLabel.");
        }

        private T RequestValue()
        {
            BlockingCollection<T> _Wait = new BlockingCollection<T>();
            lock (_Consumer)
            {
                _Consumer.Add(_Wait);

                if (!_RequestIsRunning)
                {
                    _RequestIsRunning = true;

                    T value = new T();
                    Task task = Task.Factory.StartNew(() =>
                    {
                        do
                        {
#if NET45
                            Console.WriteLine("GET: " + DateTime.Now);
#endif
                            Owner.GetLabel(this);
                        } while (!_Wait.TryTake(out value, GET_REQUEST_TIMEOUT));
                    });
                    task.Wait();
                    return value;
                }
                else
                {
                    return _Wait.Take();
                }
            }
        }

        private void SetValue(T value)
        {
            //TODO:Repeat
            Owner.SetLabel(this, ConvertToBytes(value));
        }
    }
}