using System;
using System.Linq;

namespace DwgSmsServerNet.Messages
{
    class DwgMessageHeader
    {
        public DwgMessageType Type { get; private set; }
        public byte[] Mac { get; private set; }
        public int BodyLength { get; private set; }

        public DwgMessageHeader(DwgMessageType type, byte[] mac, int bodyLength)
        {
            Type = type;
            Mac = mac;
            BodyLength = bodyLength;
        }

        public DwgMessageHeader(byte[] bytes)
        {
            byte[] tmp = new byte[2];
            Array.Copy(bytes, 20, tmp, 0, 2);
            Type = (DwgMessageType)BitConverter.ToInt16(tmp.Reverse().ToArray(), 0);

            if (Type != 0)
            {
                tmp = new byte[4];
                Array.Copy(bytes, tmp, 4);
                BodyLength = BitConverter.ToInt32(tmp.Reverse().ToArray(), 0);

                Mac = new byte[6];
                Array.Copy(bytes, 4, Mac, 0, 6);
            }
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[24];

            byte[] bodyLengthBytes = BitConverter.GetBytes(BodyLength).Reverse().ToArray();
            Array.Copy(bodyLengthBytes, 0, bytes, 0, 4);

            byte[] typeBytes = BitConverter.GetBytes((short)Type).Reverse().ToArray();
            Array.Copy(typeBytes, 0, bytes, 20, 2);

            Array.Copy(Mac, 0, bytes, 4, 6);

            return bytes;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
