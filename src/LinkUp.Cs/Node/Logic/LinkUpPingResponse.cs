namespace LinkUp.Cs.Node.Logic
{
   internal class LinkUpPingResponse : LinkUpLogic
   {
      protected override void ParseFromRaw(byte[] data)
      {
      }

      protected override byte[] ToRaw()
      {
         return new byte[] { (byte)LinkUpLogicType.PingResponse };
      }
   }
}