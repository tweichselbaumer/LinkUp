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

using LinkUp.Cs.Raw;

namespace LinkUp.Cs.Datagram
{
   public class Datagram
   {
      private byte[] _data = new byte[0];

      public Datagram(byte[] data)
      {
         _data = new byte[data.Length];
         Array.Copy(data, _data, _data.Length);
      }

      public Datagram()
      {
      }

      public static Datagram ConvertFromLinkUpPacket(LinkUpPacket packet)
      {
         return new Datagram(packet.Data);
      }

      public LinkUpPacket ConvertToLinkUpPacket()
      {
         return new LinkUpPacket() { Data = (byte[])_data.Clone() };
      }

      public void Delete(int size, int offset)
      {
         byte[] temp = (byte[])_data.Clone();

         Array.Resize(ref _data, _data.Length - size);
         if (offset > 0)
         {
            Array.Copy(temp, 0, _data, 0, offset);
         }

         if (offset + size < _data.Length)
         {
            Array.Copy(temp, offset + size, _data, offset, _data.Length - offset);
         }
      }

      public void DeleteBack(int size)
      {
         Delete(size, _data.Length - size);
      }

      public void DeleteFront(int size)
      {
         Delete(size, 0);
      }

      public byte[] Read(int size, int offset)
      {
         byte[] temp = new byte[size];

         Array.Copy(_data, offset, temp, 0, size);

         return temp;
      }

      public byte[] ReadBack(int size)
      {
         return Read(size, _data.Length - size);
      }

      public byte[] ReadFront(int size)
      {
         return Read(size, 0);
      }

      public int Size()
      {
         return _data.Length;
      }

      public void Write(byte[] data, int offset)
      {
         byte[] temp = (byte[])_data.Clone();
         Array.Resize(ref _data, _data.Length + data.Length);
         if (offset > 0)
         {
            Array.Copy(temp, 0, _data, 0, offset);
         }

         Array.Copy(data, 0, _data, offset, data.Length);

         if (offset < _data.Length - data.Length)
         {
            Array.Copy(temp, offset, _data, offset + data.Length, _data.Length - data.Length - offset);
         }
      }

      public void WriteBack(byte[] data)
      {
         Write(data, _data.Length);
      }

      public void WriteFront(byte[] data)
      {
         Write(data, 0);
      }
   }
}