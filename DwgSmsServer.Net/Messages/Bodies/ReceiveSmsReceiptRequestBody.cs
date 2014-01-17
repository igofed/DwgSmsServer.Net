using System;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages.Bodies
{
    class ReceiveSmsReceiptRequestBody : DwgMessageBody
    {
        public byte Port { get; set; }
        public string Number { get; set; }     
        public byte ReceiptId { get; set; }
        public DateTime DateTime { get; set; }
        public DwgSmsReceiptState State { get; set; }

        public ReceiveSmsReceiptRequestBody(byte[] bytes)
        {
            var readingBytes = bytes.AsEnumerable();
            
            Port = readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            Number = Encoding.ASCII.GetString(readingBytes.Take(23).ToArray()).Trim('\0');
            readingBytes = readingBytes.Skip(23);

            ReceiptId = readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            string timestamp = Encoding.ASCII.GetString(readingBytes.Take(15).ToArray()).Trim('\0');
            DateTime = DateTime.ParseExact(timestamp, "yyyyMMddHHmmss", null);
            readingBytes = readingBytes.Skip(15);

            //timezone
            readingBytes = readingBytes.Skip(1);

            var state = readingBytes.Take(1).First();
            if(state <= 31)
                State = DwgSmsReceiptState.Success;
            else if(state <=63)
                State = DwgSmsReceiptState.TemporaryError;
            else 
                State = DwgSmsReceiptState.PermanentError;

            Length = 42;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}:{3}", Port, Number, ReceiptId, State);
        }
    }
}
