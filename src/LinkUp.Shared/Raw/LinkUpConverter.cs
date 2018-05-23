using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUp.Raw
{
    public class LinkUpConverter
    {
        private byte[] _Buffer;
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
                if (_Buffer == null)
                {
                    _Buffer = data;
                }
                else
                {
                    _Buffer = _Buffer.Concat(data).ToArray();
                }
            }

            return ParseBuffer();
        }

        public byte[] ConvertToSend(LinkUpPacket packet)
        {
            return packet.ToRaw().ToArray();
        }

        private List<LinkUpPacket> ParseBuffer()
        {
            List<LinkUpPacket> result = new List<LinkUpPacket>();
            int indexOfPreamble = Array.IndexOf(_Buffer, Constant.Preamble);
            int indexOfEndOfPacket = Array.IndexOf(_Buffer, Constant.EndOfPacket);

            if (indexOfPreamble != -1 && indexOfEndOfPacket != -1 && indexOfPreamble < indexOfEndOfPacket)
            {
                int indexOfPreambleNext = Array.IndexOf(_Buffer, Constant.Preamble, indexOfPreamble + 1);
                while (indexOfPreambleNext < indexOfEndOfPacket && indexOfPreambleNext != -1)
                {
                    indexOfPreamble += _Buffer.Skip(indexOfPreamble + 1).Take(indexOfEndOfPacket - indexOfPreamble).ToList().IndexOf(Constant.Preamble) + 1;
                    indexOfPreambleNext = Array.IndexOf(_Buffer, Constant.Preamble, indexOfPreamble + 1);
                }

                //List<byte> packetRaw = _Buffer.Skip(indexOfPreamble).Take(indexOfEndOfPacket - indexOfPreamble + 1).ToList();

                byte[] packetRaw = new byte[indexOfEndOfPacket - indexOfPreamble + 1];
                Array.Copy(_Buffer, indexOfPreamble, packetRaw, 0, indexOfEndOfPacket - indexOfPreamble + 1);

                LinkUpPacket packet = LinkUpPacket.ParseFromRaw(packetRaw);
                if (packet.IsValid)
                {
                    result.Add(packet);
                    _TotalReceivedPackets++;
                }
                else
                {
                    _TotalFailedPackets++;
                }
                _Buffer = _Buffer.Skip(indexOfPreamble).ToArray();
            }
            else
            {
                return result;
            }
            if (indexOfEndOfPacket != -1)
            {
                _Buffer = _Buffer.Skip(indexOfEndOfPacket + 1).ToArray();
            }

            if (indexOfPreamble != -1)
            {
                result.AddRange(ParseBuffer());
            }

            return result;
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