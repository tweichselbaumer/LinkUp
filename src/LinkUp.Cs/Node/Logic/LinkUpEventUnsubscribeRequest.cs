using System;
using System.Linq;

namespace LinkUp.Cs.Node.Logic
{
   internal class LinkUpEventUnsubscribeRequest : LinkUpLogic
   {
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

      protected override void ParseFromRaw(byte[] data)
      {
         Identifier = BitConverter.ToUInt16(data, 1);
      }

      protected override byte[] ToRaw()
      {
         return new byte[] { (byte)LinkUpLogicType.EventUnsubscribeRequest }.Concat(BitConverter.GetBytes(Identifier)).ToArray();
      }
   }
}