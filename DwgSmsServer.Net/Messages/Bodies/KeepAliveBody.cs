namespace DwgSmsServerNet.Messages.Bodies
{
    class KeepAliveBody : DwgMessageBody
    {
        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }
}
