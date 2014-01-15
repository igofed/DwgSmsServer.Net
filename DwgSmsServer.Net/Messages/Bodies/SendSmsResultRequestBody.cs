using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages.Bodies
{
    class SendSmsResultRequestBody : DwgMessageBody
    {
        public byte CountOfNumbers { get; private set; }
        public string Number { get; private set; }
        public byte Port { get; private set; }
        public DwgSendSmsResult Result { get; private set; }
        public byte CountOfSlices { get; private set; }
        public byte SucceededSlices { get; private set; }

        public SendSmsResultRequestBody(byte[] bytes)
        {
            Length = bytes.Length;


            var readingBytes = bytes.AsEnumerable();

            CountOfNumbers = readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            Number = Encoding.ASCII.GetString(readingBytes.Take(24).ToArray()).Trim('\0');
            readingBytes = readingBytes.Skip(24);

            Port = readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            Result = (DwgSendSmsResult)readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            CountOfSlices = readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            SucceededSlices = readingBytes.Take(1).First();
        }

        public override string ToString()
        {
            return string.Format("Number: {0}; Result: {1}; {2}/{3}; ", Number, Result, SucceededSlices, CountOfSlices);
        }
    }
}
