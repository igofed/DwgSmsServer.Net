using System;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages.Bodies
{
    class ReceiveSmsMessageRequestBody : DwgMessageBody
    {
        public string Number { get; private set; }
        public byte Port { get; private set; }
        public DateTime DateTime { get; private set; }
        public byte Timezone { get; private set; }
        public short ContentLength { get; private set; }
        public string Content { get; private set; }

        public ReceiveSmsMessageRequestBody(byte[] bytes)
        {
            var readingBytes = bytes.AsEnumerable();

            Number = Encoding.ASCII.GetString(readingBytes.Take(24).ToArray()).Trim('\0');
            readingBytes = readingBytes.Skip(24);

            //message type always SMS (0)
            readingBytes = readingBytes.Skip(1);

            Port = readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            string timestamp = Encoding.ASCII.GetString(readingBytes.Take(15).ToArray()).Trim('\0');
            DateTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmss", null);
            readingBytes = readingBytes.Skip(15);

            Timezone = readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            DwgSmsEncoding smsEncoding = (DwgSmsEncoding)readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            ContentLength = BitConverter.ToInt16(readingBytes.Take(2).Reverse().ToArray(), 0);
            readingBytes = readingBytes.Skip(2);

            Encoding encoding = smsEncoding == DwgSmsEncoding.Unicode ? Encoding.BigEndianUnicode : new Gsm7BitEncoding();
            Content = encoding.GetString(readingBytes.Take(ContentLength).ToArray());

            Length = 45 + ContentLength;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Number, Content);
        }
    }
}
