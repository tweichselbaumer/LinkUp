/********************************************************************************
 * MIT License
 *
 * Copyright (c) 2023 Thomas Weichselbaumer
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 ********************************************************************************/

namespace LinkUp.Cs.Node
{
   public class LinkUpPropertyLabel<T> : LinkUpPropertyLabelBase
      where T : IConvertible, new()
   {
      private T _Value;

      public LinkUpPropertyLabel()
      {
      }

      public T Value
      {
         get
         {
            T result;

            if (Owner == null)
            {
               result = _Value;
            }
            else
            {
               RequestValue();
               result = _Value;
            }

            return result;
         }

         set
         {
            if (Owner == null)
               _Value = value;
            else
               SetValue(value);
         }
      }

      public override object ValueObject
      {
         get
         {
            return Value;
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
            return LinkUpLabelType.Property;
         }
      }

      public override void Dispose()
      {
      }

      internal override void GetDone(byte[] data)
      {
         _Value = (T)ConvertFromBytes(data);
         base.GetDone(data);
      }

      internal override void SetDone()
      {
         base.SetDone();
      }

      protected override byte[] ConvertToBytes(object value)
      {
         if (_Value is bool)
         {
            return BitConverter.GetBytes((bool)value);
         }
         if (_Value is sbyte)
         {
            return new byte[] { (byte)value };
         }
         if (_Value is byte)
         {
            return new byte[] { (byte)value };
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
   }

   public class LinkUpPropertyLabel_Binary : LinkUpPropertyLabelBase
   {
      private byte[] _Value;

      public LinkUpPropertyLabel_Binary()
      {
      }

      public byte[] Value
      {
         get
         {
            byte[] result;

            if (Owner == null)
            {
               result = _Value;
            }
            else
            {
               RequestValue();
               result = _Value;
            }

            return result;
         }

         set
         {
            if (Owner == null)
               _Value = value;
            else
               SetValue(value);
         }
      }

      public override object ValueObject
      {
         get
         {
            return Value;
         }
      }

      internal override byte[] Data
      {
         set
         {
            Value = value;
         }

         get
         {
            return Value;
         }
      }

      internal override LinkUpLabelType LabelType
      {
         get
         {
            return LinkUpLabelType.Property;
         }
      }

      public override void Dispose()
      {
      }

      internal override void GetDone(byte[] data)
      {
         _Value = data;
         base.GetDone(data);
      }

      internal override void SetDone()
      {
         base.SetDone();
      }

      protected override byte[] ConvertToBytes(object value)
      {
         if (value is byte[])
            return (byte[])value;
         else
            return null;
      }
   }

   public abstract class LinkUpPropertyLabelBase : LinkUpLabel
   {
      private const int GET_REQUEST_TIMEOUT = 4000;
      private const int SET_REQUEST_TIMEOUT = 2000;
      private AutoResetEvent _GetAutoResetEvent = new AutoResetEvent(false);
      private AutoResetEvent _SetAutoResetEvent = new AutoResetEvent(false);

      public abstract object ValueObject
      {
         get;
      }

      internal abstract byte[] Data { get; set; }

      public static LinkUpPropertyLabelBase CreateNew(byte[] options)
      {
         if (options.Length > 0)
         {
            LinkUpPropertyType type = (LinkUpPropertyType)options[0];
            switch (type)
            {
               case LinkUpPropertyType.Boolean:
                  return new LinkUpPropertyLabel<bool>();

               case LinkUpPropertyType.Int8:
                  return new LinkUpPropertyLabel<byte>();

               case LinkUpPropertyType.Double:
                  return new LinkUpPropertyLabel<double>();

               case LinkUpPropertyType.Int16:
                  return new LinkUpPropertyLabel<short>();

               case LinkUpPropertyType.Int32:
                  return new LinkUpPropertyLabel<int>();

               case LinkUpPropertyType.Int64:
                  return new LinkUpPropertyLabel<long>();

               case LinkUpPropertyType.UInt8:
                  return new LinkUpPropertyLabel<sbyte>();

               case LinkUpPropertyType.Single:
                  return new LinkUpPropertyLabel<float>();

               case LinkUpPropertyType.UInt16:
                  return new LinkUpPropertyLabel<ushort>();

               case LinkUpPropertyType.UInt32:
                  return new LinkUpPropertyLabel<uint>();

               case LinkUpPropertyType.UInt64:
                  return new LinkUpPropertyLabel<ulong>();

               case LinkUpPropertyType.Binary:
                  return new LinkUpPropertyLabel_Binary();

               default:
                  return null;
            }
         }
         return null;
      }

      internal virtual void GetDone(byte[] data)
      {
         _GetAutoResetEvent.Set();
      }

      internal virtual void SetDone()
      {
         _SetAutoResetEvent.Set();
      }

      protected abstract byte[] ConvertToBytes(object value);

      protected void RequestValue()
      {
         _GetAutoResetEvent.Reset();
         Owner.GetProperty(this);
         if (!_GetAutoResetEvent.WaitOne(GET_REQUEST_TIMEOUT))
            throw new Exception(string.Format("Unable to get label: {0}.", Name));
      }

      protected void SetValue(object value)
      {
         _SetAutoResetEvent.Reset();
         Owner.SetProperty(this, ConvertToBytes(value));
         if (!_SetAutoResetEvent.WaitOne(SET_REQUEST_TIMEOUT))
            throw new Exception(string.Format("Unable to set label: {0}.", Name));
      }
   }
}