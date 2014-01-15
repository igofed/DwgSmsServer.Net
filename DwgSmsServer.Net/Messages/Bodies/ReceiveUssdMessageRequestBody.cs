using System;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages.Bodies
{
    class ReceiveUssdMessageRequestBody : DwgMessageBody
    {
        public byte Port { get; set; }
        public DwgRecieveUssdResult Status { get; set; }
        public short ContentLength { get; set; }
        public string Content { get; set; }

        public ReceiveUssdMessageRequestBody(byte[] bytes)
        {
            Length = bytes.Length;

            Port = bytes[0];
            Status = (DwgRecieveUssdResult)bytes[1];

            ContentLength = BitConverter.ToInt16(bytes, 2);

            //skip 5 because of 5 bit is encoding, which always Unicode
            Content = Encoding.BigEndianUnicode.GetString(bytes.Skip(5).Take(ContentLength).ToArray());
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", Status, Content);
        }
    }
}
