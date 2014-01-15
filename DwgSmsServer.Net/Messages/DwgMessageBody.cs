using System;

namespace DwgSmsServerNet.Messages
{
    abstract class DwgMessageBody
    {
        public int Length { get; protected set; }

        public virtual byte[] ToBytes()
        {
            throw new NotSupportedException();
        }
    }
}
