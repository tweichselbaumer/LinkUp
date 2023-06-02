using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUp.Cs.Raw
{
   public class LinkUpPacket
   {
      private byte[] _Data;
      private bool _IsValid;

      public ushort Crc
      {
         get
         {
            return Crc16(_Data);
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

      public int Length
      {
         get
         {
            return _Data == null ? 0 : _Data.Length;
         }
      }

      internal bool IsValid
      {
         get
         {
            return _IsValid;
         }
      }

      internal static LinkUpPacket ParseFromRaw(byte[] data, int startIndex, int size)
      {
         LinkUpPacket result = new LinkUpPacket();
         int length = 0;
         try
         {
            int escaped = 0;

            length = BitConverter.ToInt32(RemoveEscaping(data, startIndex + 1, 4, ref escaped), 0);

            result.Data = RemoveEscaping(data, startIndex + 5 + escaped, length, ref escaped);

            ushort crc = BitConverter.ToUInt16(RemoveEscaping(data, startIndex + 5 + length + escaped, 2, ref escaped), 0);

            if (!(crc == result.Crc) || data[startIndex] != Constant.Preamble || data[startIndex + size - 1] != Constant.EndOfPacket || length != size - (4 + 2 + 1 + 1 + escaped))
            {
               result._IsValid = false;
            }
            else
            {
               result._IsValid = true;
            }
         }
         catch (Exception ex)
         {
            result._IsValid = false;
         }

         return result;
      }

      internal byte[] ToRaw()
      {
         byte[] lenghtData = AddEscaping(BitConverter.GetBytes(Length));
         byte[] dataData = AddEscaping(Data);
         byte[] crcData = null;

         if (Length < 1024)
            crcData = AddEscaping(BitConverter.GetBytes(Crc));
         else
            crcData = AddEscaping(BitConverter.GetBytes((ushort)0));

         int lenghtSize = lenghtData.Length;
         int dataSize = dataData.Length;
         int crcSize = crcData.Length;

         int size = 1 + lenghtSize + dataSize + crcSize + 1;

         byte[] result = new byte[size];
         result[0] = Constant.Preamble;

         Array.Copy(lenghtData, 0, result, 1, lenghtSize);
         Array.Copy(dataData, 0, result, 1 + lenghtSize, dataSize);
         Array.Copy(crcData, 0, result, 1 + lenghtSize + dataSize, crcSize);

         result[size - 1] = Constant.EndOfPacket;

         return result;
      }

      internal byte[] ToRaw2()
      {
         List<byte> result = new List<byte>();

         result.Add(Constant.Preamble);
         result.AddRange(AddEscaping2(BitConverter.GetBytes(Length).ToList()));
         result.AddRange(AddEscaping2(Data.ToList()));
         if (Length < 1024)
            result.AddRange(AddEscaping2(BitConverter.GetBytes(Crc).ToList()));
         else
            result.AddRange(AddEscaping2(BitConverter.GetBytes((ushort)0).ToList()));
         result.Add(Constant.EndOfPacket);

         return result.ToArray();
      }

      private static ushort Crc16(byte[] bytes)
      {
         const ushort poly = 4129;
         ushort[] table = new ushort[256];
         ushort initialValue = 0x0;
         ushort temp, a;
         ushort crc = initialValue;
         for (int i = 0; i < table.Length; ++i)
         {
            temp = 0;
            a = (ushort)(i << 8);
            for (int j = 0; j < 8; ++j)
            {
               if (((temp ^ a) & 0x8000) != 0)
                  temp = (ushort)(temp << 1 ^ poly);
               else
                  temp <<= 1;
               a <<= 1;
            }
            table[i] = temp;
         }
         for (int i = 0; i < bytes.Length; ++i)
         {
            crc = (ushort)(crc << 8 ^ table[crc >> 8 ^ 0xff & bytes[i]]);
         }
         return crc;
      }

      private static byte[] RemoveEscaping(byte[] data, int startIndex, int size, ref int escaped)
      {
         int indexOfSkipPattern = Array.IndexOf(data, Constant.SkipPattern, startIndex);

         int localEscaped = 0;

         if (indexOfSkipPattern == -1 || indexOfSkipPattern > size + startIndex)
         {
            byte[] result = new byte[size];
            Array.Copy(data, startIndex, result, 0, size);
            return result;
         }
         else
         {
            byte[] result = new byte[size];
            int j = 0;
            int i = startIndex;
            while (indexOfSkipPattern != -1 && indexOfSkipPattern < size + startIndex + localEscaped)
            {
               if (indexOfSkipPattern - i >= 0)
               {
                  Array.Copy(data, i, result, j, indexOfSkipPattern - i);
                  j += indexOfSkipPattern - i;
               }

               i = indexOfSkipPattern + 1;
               result[j] = (byte)(data[i] ^ Constant.XorValue);
               localEscaped++;
               i++;
               j++;

               indexOfSkipPattern = Array.IndexOf(data, Constant.SkipPattern, i);
            }

            if (size - j > 0)
            {
               Array.Copy(data, i, result, j, size - j);
            }

            escaped += localEscaped;

            return result;
         }
      }

      private static byte[] RemoveEscaping2(byte[] data)
      {
         int length = data.Length;
         byte[] result = new byte[data.Length];
         int j = 0;

         for (int i = 0; i < length; i++)
         {
            if (data[i] == Constant.SkipPattern)
            {
               i++;
               result[j] = (byte)(data[i] ^ Constant.XorValue);
            }
            else
            {
               result[j] = data[i];
            }
            j++;
         }

         if (j < data.Length - 1)
         {
            byte[] temp = new byte[j];
            Array.Copy(result, temp, j);
            result = temp;
         }

         return result;
      }

      private byte[] AddEscaping(byte[] data)
      {
         int count = data.Length;
         byte[] result = new byte[count * 2];

         int j = 0;

         for (int i = 0; i < count; i++)
         {
            byte value = data[i];
            if (value == Constant.Preamble || value == Constant.EndOfPacket || value == Constant.SkipPattern)
            {
               result[j++] = Constant.SkipPattern;
               result[j++] = (byte)(value ^ Constant.XorValue);
            }
            else
            {
               result[j++] = value;
            }
         }
         byte[] tmp = new byte[j];
         Array.Copy(result, tmp, j);
         return tmp;
      }

      private List<byte> AddEscaping2(List<byte> data)
      {
         List<byte> result = new List<byte>();

         for (int i = 0; i < data.Count; i++)
         {
            if (data[i] == Constant.Preamble || data[i] == Constant.EndOfPacket || data[i] == Constant.SkipPattern)
            {
               result.Add(Constant.SkipPattern);
               result.Add((byte)(data[i] ^ Constant.XorValue));
            }
            else
            {
               result.Add(data[i]);
            }
         }
         return result;
      }
   }
}