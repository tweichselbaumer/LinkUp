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

namespace LinkUp.Cs.Raw
{
   internal class Constant
   {
      public const byte EndOfPacket = 0x99;
      public const byte Preamble = 0xAA;
      public const byte SkipPattern = 0x55;
      public const byte XorValue = 0x20;
   }

   internal class Converter
   {
      private byte[] _Buffer;
      private int _BufferSize;
      private int _TotalFailedPackets;
      private int _TotalReceivedPackets;

      public int TotalFailedPackets
      {
         get
         {
            return _TotalFailedPackets;
         }
      }

      public int TotalReceivedPackets
      {
         get
         {
            return _TotalReceivedPackets;
         }
      }

      private void AddBufferEnd()
      {
         if (_BufferSize < _Buffer.Length)
            _Buffer[_BufferSize] = Constant.Preamble;
         if (_BufferSize + 1 < _Buffer.Length)
            _Buffer[_BufferSize + 1] = Constant.EndOfPacket;
      }

      private int IndexOfInBuffer(int startIndex, byte value)
      {
         int indexOf = Array.IndexOf(_Buffer, value, startIndex);
         if (indexOf > _BufferSize)
         {
            indexOf = -1;
         }
         return indexOf;
      }

      private List<Packet> ParseBuffer()
      {
         List<Packet> result = new List<Packet>();
         int indexOfPreamble = IndexOfInBuffer(0, Constant.Preamble);
         int indexOfEndOfPacket = IndexOfInBuffer(0, Constant.EndOfPacket);

         if (indexOfPreamble != -1 && indexOfEndOfPacket != -1 && indexOfPreamble < indexOfEndOfPacket)
         {
            int indexOfPreambleNext = IndexOfInBuffer(indexOfPreamble + 1, Constant.Preamble);

            while (indexOfPreambleNext < indexOfEndOfPacket && indexOfPreambleNext != -1)
            {
               indexOfPreamble += _Buffer.Skip(indexOfPreamble + 1).Take(indexOfEndOfPacket - indexOfPreamble).ToList().IndexOf(Constant.Preamble) + 1;
               indexOfPreambleNext = IndexOfInBuffer(indexOfPreamble + 1, Constant.Preamble);
            }

            Packet packet = Packet.ParseFromRaw(_Buffer, indexOfPreamble, indexOfEndOfPacket - indexOfPreamble + 1);
            if (packet.IsValid)
            {
               result.Add(packet);
               _TotalReceivedPackets++;
            }
            else
            {
               _TotalFailedPackets++;
            }
         }

         if (indexOfEndOfPacket != -1)
         {
            //if (_Buffer.Length > indexOfEndOfPacket + 1)
            //{
            _BufferSize -= indexOfEndOfPacket + 1;

            Array.Copy(_Buffer, indexOfEndOfPacket + 1, _Buffer, 0, _BufferSize);
            AddBufferEnd();
            //}
            //else
            //{
            //    _Buffer = new byte[0];
            //    _BufferSize = 0;
            //}
         }

         if (indexOfPreamble != -1 && indexOfEndOfPacket != -1)
         {
            result.AddRange(ParseBuffer());
         }

         return result;
      }

      public List<Packet> ConvertFromReceived(byte[] data)
      {
         if (data.Length <= 0)
         {
            throw new ArgumentException("Length must be greater zero.");
         }
         else
         {
            if (_Buffer == null || _Buffer.Length == 0)
            {
               _Buffer = data;
               _BufferSize = data.Length;
            }
            else
            {
               if (_Buffer.Length < _BufferSize + data.Length)
               {
                  Array.Resize(ref _Buffer, _BufferSize + data.Length);
               }
               else if ((_BufferSize + data.Length + 1024) * 10 < _Buffer.Length)
               {
                  Array.Resize(ref _Buffer, (_BufferSize + data.Length) * 2);
               }
               Array.Copy(data, 0, _Buffer, _BufferSize, data.Length);
               _BufferSize += data.Length;
               AddBufferEnd();
            }
         }

         return ParseBuffer();
      }

      public byte[] ConvertToSend(Packet packet)
      {
         return packet.ToRaw();
      }
   }
}