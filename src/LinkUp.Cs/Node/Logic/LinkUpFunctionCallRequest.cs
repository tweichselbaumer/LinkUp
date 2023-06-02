using System;
using System.Linq;

namespace LinkUp.Cs.Node.Logic
{
   internal class LinkUpFunctionCallRequest : LinkUpLogic
   {
      private byte[] _Data;
      private ushort _Identifier;

      public ushort Identifier
      {
         get
         {
            return _Identifier;
         }

         set
         {
            _Identifier = value;
         }
      }

      public byte[] Data
      {
         get
         {
            return _Data;
         }

         set
         {
            _Data = value;
         }
      }

      protected override void ParseFromRaw(byte[] data)
      {
         Identifier = BitConverter.ToUInt16(data, 1);
         _Data = new byte[data.Length - 3];
         Array.Copy(data, 3, _Data, 0, data.Length - 3);
      }

      protected override byte[] ToRaw()
      {
         byte[] tmp = new byte[_Data.Length + 1 + 2];
         tmp[0] = (byte)LinkUpLogicType.FunctionCallRequest;
         Array.Copy(BitConverter.GetBytes(Identifier), 0, tmp, 1, 2);
         Array.Copy(_Data, 0, tmp, 3, _Data.Length);
         return tmp;
         //return new byte[] { (byte)LinkUpLogicType.FunctionCallRequest }.Concat(BitConverter.GetBytes(Identifier)).Concat(_Data).ToArray();
      }
   }
}