using System.Linq;

namespace DwgSmsServerNet.Messages.Bodies
{
    class CsqRssiRequestBody : DwgMessageBody
    {
        public byte CountOfPorts { get; private set; }
        public byte[] PortStatuses { get; private set; }

        public CsqRssiRequestBody(byte[] bytes)
        {
            Length = bytes.Length;

            CountOfPorts = bytes.Take(1).First();
            PortStatuses = bytes.Skip(1).ToArray();
        }

        public override string ToString()
        {
            return string.Join(",", PortStatuses);
        }
    }
}
