using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUp.Portable
{
    public class LinkUpConverter
    {
        private List<byte> _Buffer;
        private int _TotalFailedPackets;
        private int _TotalReceivedPackets;

        public int TotalFailedPackets
        {
            get
            {
                return _TotalFailedPackets;
            }

            set
            {
                _TotalFailedPackets = value;
            }
        }

        public int TotalReceivedPackets
        {
            get
            {
                return _TotalReceivedPackets;
            }
            set
            {
                _TotalReceivedPackets = value;
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
                    _Buffer = new List<byte>();
                }
                _Buffer.AddRange(data.ToList());
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
            int indexOfPreamble = _Buffer.IndexOf(Constant.Preamble);
            int indexOfEndOfPacket = _Buffer.IndexOf(Constant.EndOfPacket);

            if (indexOfPreamble != -1 && indexOfEndOfPacket != -1 && indexOfPreamble < indexOfEndOfPacket)
            {
                while (_Buffer.Skip(indexOfPreamble + 1).Take(indexOfEndOfPacket - indexOfPreamble).Contains(Constant.Preamble))
                {
                    indexOfPreamble += _Buffer.Skip(indexOfPreamble + 1).Take(indexOfEndOfPacket - indexOfPreamble).ToList().IndexOf(Constant.Preamble) + 1;
                }
                List<byte> packetRaw = _Buffer.Skip(indexOfPreamble).Take(indexOfEndOfPacket - indexOfPreamble + 1).ToList();

                LinkUpPacket packet = LinkUpPacket.ParseFromRaw(packetRaw);
                if (packet.IsValid)
                {
                    result.Add(packet);
                    TotalReceivedPackets++;
                }
                else
                {
                    TotalFailedPackets++;
                }
                _Buffer = _Buffer.Skip(indexOfPreamble + 1).ToList();
            }
            if (indexOfEndOfPacket != -1)
            {
                _Buffer = _Buffer.Skip(indexOfEndOfPacket + 1).ToList();
            }

            if (_Buffer.Contains(Constant.EndOfPacket))
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