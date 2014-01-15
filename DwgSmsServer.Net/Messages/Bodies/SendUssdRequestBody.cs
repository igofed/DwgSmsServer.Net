using System;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages.Bodies
{
    class SendUssdRequestBody : DwgMessageBody
    {
        public byte Port { get; private set; }
        public DwgUssdType Type { get; private set; }
        public short ContentLength { get; private set; }
        public string Content { get; private set; }

        public SendUssdRequestBody(byte port, DwgUssdType type, string content)
        {
            Port = port;
            Content = content;
            Type = type;
            ContentLength = (short)Encoding.ASCII.GetByteCount(content);
            Length = 4 + ContentLength;
        }

        public override byte[] ToBytes()
        {
            byte[] info = { Port, (byte)Type };
            var contentLengthBytes = BitConverter.GetBytes(ContentLength).Reverse();
            var contentBytes = Encoding.ASCII.GetBytes(Content);

            return info.Concat(contentLengthBytes).Concat(contentBytes).ToArray();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Type, Content);
        }
    }
}
