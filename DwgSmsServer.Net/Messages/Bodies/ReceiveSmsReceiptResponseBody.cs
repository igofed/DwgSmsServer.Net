namespace DwgSmsServerNet.Messages.Bodies
{
    class ReceiveSmsReceiptResponseBody : DwgMessageBody
    {
        public DwgMessageResult DwgMessageResult { get; private set; }

        public ReceiveSmsReceiptResponseBody(DwgMessageResult dwgMessageResult)
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
