namespace DwgSmsServerNet.Messages.Bodies
{
    class SendSmsResponseBody : DwgMessageBody
    {
        public DwgSendSmsResult Result { get; private set; }

        public SendSmsResponseBody(byte[] bytes)
        {
            Result = (DwgSendSmsResult)bytes[0];
        }

        public override string ToString()
        {
            return Result.ToString();
        }
    }
}
