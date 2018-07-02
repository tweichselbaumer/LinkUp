using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUp.Raw
{
    public class LinkUpConverter
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

        public List<LinkUpPacket> ConvertFromReceived(byte[] data)
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

        private void AddBufferEnd()
        {
            if (_BufferSize < _Buffer.Length)
                _Buffer[_BufferSize] = Constant.Preamble;
            if (_BufferSize + 1 < _Buffer.Length)
                _Buffer[_BufferSize + 1] = Constant.EndOfPacket;
        }

        public byte[] ConvertToSend(LinkUpPacket packet)
        {
            return packet.ToRaw().ToArray();
        }

        private List<LinkUpPacket> ParseBuffer()
        {
            List<LinkUpPacket> result = new List<LinkUpPacket>();
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

                LinkUpPacket packet = LinkUpPacket.ParseFromRaw(_Buffer, indexOfPreamble, indexOfEndOfPacket - indexOfPreamble + 1);
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
                _BufferSize -= (indexOfEndOfPacket + 1);

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

        private int IndexOfInBuffer(int startIndex, byte value)
        {
            int indexOf = Array.IndexOf(_Buffer, value, startIndex);
            if (indexOf > _BufferSize)
            {
                indexOf = -1;
            }
            return indexOf;
        }
    }

    internal class Constant
    {
        public const byte EndOfPacket = 0x99;
        public const byte Preamble = 0xAA;
        public const byte SkipPattern = 0x55;
        public const byte XorValue = 0x20;
    }
}