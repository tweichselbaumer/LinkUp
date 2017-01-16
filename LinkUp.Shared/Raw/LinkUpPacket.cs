using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUp.Raw
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

        public ushort Length
        {
            get
            {
                return _Data == null ? (ushort)0 : Convert.ToUInt16(_Data.Length);
            }
        }

        internal bool IsValid
        {
            get
            {
                return _IsValid;
            }
        }

        internal static LinkUpPacket ParseFromRaw(List<byte> data)
        {
            LinkUpPacket result = new LinkUpPacket();

            try
            {
                data = RemoveEscaping(data);
                ushort length = BitConverter.ToUInt16(data.ToArray(), 1);
                result.Data = data.Skip(3).Take(length).ToArray();
                ushort crc = BitConverter.ToUInt16(data.ToArray(), 3 + length);
                if (crc != result.Crc || data[0] != Constant.Preamble || data[data.Count - 1] != Constant.EndOfPacket)
                {
                    result._IsValid = false;
                }
                else
                {
                    result._IsValid = true;
                }
            }
            catch (Exception)
            {
                result._IsValid = false;
            }

            return result;
        }

        internal List<byte> ToRaw()
        {
            List<byte> result = new List<byte>();

            result.Add(Constant.Preamble);
            result.AddRange(AddEscaping(BitConverter.GetBytes(Length).ToList()));
            result.AddRange(AddEscaping(Data.ToList()));
            result.AddRange(AddEscaping(BitConverter.GetBytes(Crc).ToList()));
            result.Add(Constant.EndOfPacket);

            return result;
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
                        temp = (ushort)((temp << 1) ^ poly);
                    else
                        temp <<= 1;
                    a <<= 1;
                }
                table[i] = temp;
            }
            for (int i = 0; i < bytes.Length; ++i)
            {
                crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & bytes[i]))]);
            }
            return crc;
        }

        private static List<byte> RemoveEscaping(List<byte> data)
        {
            List<byte> result = new List<byte>();

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i] == Constant.SkipPattern)
                {
                    i++;
                    result.Add((byte)(data[i] ^ Constant.XorValue));
                }
                else
                {
                    result.Add(data[i]);
                }
            }
            return result;
        }

        private List<byte> AddEscaping(List<byte> data)
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