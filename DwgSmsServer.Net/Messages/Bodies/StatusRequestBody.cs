using System.Linq;

namespace DwgSmsServerNet.Messages.Bodies
{
    class StatusRequestBody : DwgMessageBody
    {
        public byte PortsCount { get; private set; }
        public DwgPortStatus[] PortsStatuses { get; private set; }

        public StatusRequestBody(byte[] bytes)
        {
            Length = bytes.Length;

            PortsCount = bytes.Take(1).First();
            PortsStatuses = bytes.Skip(1).Select(status => (DwgPortStatus)status).ToArray();
        }

        public override string ToString()
        {
            return string.Join(",", PortsStatuses);
        }
    }
}
