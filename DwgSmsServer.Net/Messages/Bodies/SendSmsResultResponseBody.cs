namespace DwgSmsServerNet.Messages.Bodies
{
    class SendSmsResultResponseBody : DwgMessageBody
    {
        public DwgMessageResult DwgMessageResult { get; private set; }

        public SendSmsResultResponseBody(DwgMessageResult dwgMessageResult)
        {
            Length = 1;

            DwgMessageResult = dwgMessageResult;
        }

        public override byte[] ToBytes()
        {
            return new byte[] { (byte)DwgMessageResult };
        }

        public override string ToString()
        {
            return DwgMessageResult.ToString();
        }
    }
}
