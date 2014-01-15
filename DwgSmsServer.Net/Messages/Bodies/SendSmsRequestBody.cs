using System;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages.Bodies
{
    class SendSmsRequestBody : DwgMessageBody
    {
        public byte Port { get; private set; }
        public string Number { get; private set; }
        public string Message { get; private set; }
        public short ContentLength { get; private set; }
        public DwgSmsEncoding SmsEncoding { get; private set; }

        private Encoding _encoding;

        public SendSmsRequestBody(byte port, string number, string message, DwgSmsEncoding smsEncoding)
        {
            Port = port;
            Number = number;
            Message = message;
            SmsEncoding = smsEncoding;

            _encoding = smsEncoding == DwgSmsEncoding.Unicode ? Encoding.BigEndianUnicode : new Gsm7BitEncoding();

            ContentLength = (short)_encoding.GetByteCount(message);

            Length = 30 + ContentLength; //30 byte = port + number  + message
        }

        public override byte[] ToBytes()
        {
            //Port number
            //encding = always Unicode
            //message type = always SMS
            //ncountofnumbers = always 1 number to sms
            byte[] infoBytes = { Port, 1, 0, 1 };
            var numberBytes = Encoding.ASCII.GetBytes(Number).Concat(new byte[24 - Number.Length]);
            var messageLengthBytes = BitConverter.GetBytes(ContentLength).Reverse();

            var messageBytes = _encoding.GetBytes(Message);

            return infoBytes.Concat(numberBytes).Concat(messageLengthBytes).Concat(messageBytes).ToArray();
        }

        public override string ToString()
        {
            return string.Format("Number: {0}; Message: {1}", Number, Message);
        }
    }
}
