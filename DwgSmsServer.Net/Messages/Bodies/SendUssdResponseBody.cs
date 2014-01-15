using System.Linq;

namespace DwgSmsServerNet.Messages.Bodies
{
    class SendUssdResponseBody : DwgMessageBody
    {
        public DwgSendUssdResult Result { get; private set; }

        public SendUssdResponseBody(byte[] bytes)
        {
            Length = 1;

            Result = (DwgSendUssdResult)bytes.First();
        }

        public override string ToString()
        {
            return Result.ToString();
        }
    }
}
