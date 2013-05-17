using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages
{
    class DwgMessage
    {
        public MessageHeader Header { get; private set; }
        public MessageBody Body { get; private set; }

        public DwgMessage(MessageBody body, byte[] mac)
        {
            MessageType? type = null;

            if (body is AuthenticationRequestBody)
            {
                type = MessageType.AuthenticationRequest;
            }
            else if (body is AuthenticationResponseBody)
            {
                type = MessageType.AuthenticationResponse;
            }
            else if (body is CsqRssiRequestBody)
            {
                type = MessageType.CsqRssiRequest;
            }
            else if (body is CsqRssiResponseBody)
            {
                type = MessageType.CsqRssiResponse;
            }
            else if (body is StatusRequestBody)
            {
                type = MessageType.StatusRequest;
            }
            else if (body is StatusResponseBody)
            {
                type = MessageType.StatusResponse;
            }
            else if (body is SendSmsRequestBody)
            {
                type = MessageType.SendSmsRequest;
            }
            else if (body is SendSmsResponseBody)
            {
                type = MessageType.SendSmsResponse;
            }
            else if (body is SendSmsResultRequestBody)
            {
                type = MessageType.SendSmsResultRequest;
            }
            else if (body is SendSmsResultResponseBody)
            {
                type = MessageType.SendSmsResultResponse;
            }

            if (!type.HasValue)
                throw new NotSupportedException("This type of messages not supported");

            Header = new MessageHeader(type.Value, new byte[] { 0, 0, 0, 0, 0, 0 }, body.Length); ;

            Body = body;
        }

        public DwgMessage(byte[] bytes)
        {
            var readingBytes = bytes.AsEnumerable();

            Header = new MessageHeader(readingBytes.Take(24).ToArray());
            readingBytes = readingBytes.Skip(24);

            switch (Header.Type)
            {
                case MessageType.AuthenticationRequest:
                    Body = new AuthenticationRequestBody(readingBytes.Take(Header.BodyLength).ToArray());
                    break;
                case MessageType.StatusRequest:
                    Body = new StatusRequestBody(readingBytes.Take(Header.BodyLength).ToArray());
                    break;
                case MessageType.CsqRssiRequest:
                    Body = new CsqRssiRequestBody(readingBytes.Take(Header.BodyLength).ToArray());
                    break;
                case MessageType.SendSmsResponse:
                    Body = new SendSmsResponseBody(readingBytes.Take(Header.BodyLength).ToArray());
                    break;
                case MessageType.SendSmsResultRequest:
                    Body = new SendSmsResultRequestBody(readingBytes.Take(Header.BodyLength).ToArray());
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public byte[] ToBytes()
        {
            byte[] headerBytes = Header.ToBytes();
            byte[] bodyBytes = Body.ToBytes();

            return headerBytes.Concat(bodyBytes).ToArray();
        }

        public override string ToString()
        {
            return String.Format("Header: {0}; Body: {1}", Header, Body);
        }
    }

    class MessageHeader
    {
        public MessageType Type { get; set; }
        public byte[] Mac { get; set; }
        public int BodyLength { get; set; }

        public MessageHeader(MessageType type, byte[] mac, int bodyLength)
        {
            Type = type;
            Mac = mac;
            BodyLength = bodyLength;
        }

        public MessageHeader(byte[] bytes)
        {
            byte[] tmp = new byte[2];
            Array.Copy(bytes, 20, tmp, 0, 2);
            Type = (MessageType)BitConverter.ToInt16(tmp.Reverse().ToArray(), 0);

            if (Type != 0)
            {
                tmp = new byte[4];
                Array.Copy(bytes, tmp, 4);
                BodyLength = BitConverter.ToInt32(tmp.Reverse().ToArray(), 0);

                Mac = new byte[6];
                Array.Copy(bytes, 4, Mac, 0, 6);
            }
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[24];

            byte[] bodyLengthBytes = BitConverter.GetBytes(BodyLength).Reverse().ToArray();
            Array.Copy(bodyLengthBytes, 0, bytes, 0, 4);

            byte[] typeBytes = BitConverter.GetBytes((short)Type).Reverse().ToArray();
            Array.Copy(typeBytes, 0, bytes, 20, 2);

            Array.Copy(Mac, 0, bytes, 4, 6);

            return bytes;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    abstract class MessageBody
    {
        public int Length { get; protected set; }

        public virtual byte[] ToBytes()
        {
            throw new NotSupportedException();
        }
    }
    enum MessageType : byte
    {
        SendSmsRequest = 0x01,
        SendSmsResponse = 0x02,

        SendSmsResultRequest = 0x03,
        SendSmsResultResponse = 0x04,

        StatusRequest = 0x07,
        StatusResponse = 0x08,

        CsqRssiRequest = 0x0D,
        CsqRssiResponse = 0x0E,

        AuthenticationRequest = 0x0F,
        AuthenticationResponse = 0x10
    }
    public enum SendSmsResult : byte
    {
        Succeed = 0,
        Fail = 1,
        Timeout = 2,
        BadRequest = 3,
        PortUnavailable = 4,
        PartialSucceed = 5,
        OtherRrror = 255
    }
    public enum PortStatus : byte
    {
        Works = 0,
        NoSimCardInserted = 1,
        NotRegistered = 2,
        Unavailable = 3
    }
    enum Result : byte
    {
        Succeed = 0,
        Fail = 1
    }

    class AuthenticationRequestBody : MessageBody
    {
        public AuthenticationRequestBody(byte[] bytes)
        {
            User = Encoding.ASCII.GetString(bytes.Take(16).ToArray()).Trim('\0');
            Password = Encoding.ASCII.GetString(bytes.Skip(16).Take(16).ToArray()).Trim('\0');
        }

        public string User { get; set; }
        public string Password { get; set; }

        public override string ToString()
        {
            return string.Format("Login:{0}; Password:{1}", User, Password);
        }
    }
    class AuthenticationResponseBody : MessageBody
    {
        public Result Result { get; protected set; }

        public AuthenticationResponseBody(Result result)
        {
            Length = 1;

            Result = result;
        }

        public override byte[] ToBytes()
        {
            return new byte[] { (byte)Result };
        }

        public override string ToString()
        {
            return Result.ToString();
        }
    }

    class CsqRssiRequestBody : MessageBody
    {
        public byte CountOfPorts { get; set; }
        public byte[] PortStatuses { get; set; }

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
    class CsqRssiResponseBody : MessageBody
    {
        public Result Result { get; protected set; }

        public CsqRssiResponseBody(Result result)
        {
            Length = 1;

            Result = result;
        }

        public override byte[] ToBytes()
        {
            return new byte[] { (byte)Result };
        }

        public override string ToString()
        {
            return Result.ToString();
        }
    }

    class StatusRequestBody : MessageBody
    {
        public byte PortsCount { get; set; }
        public PortStatus[] PortsStatuses { get; set; }

        public StatusRequestBody(byte[] bytes)
        {
            Length = bytes.Length;

            PortsCount = bytes.Take(1).First();
            PortsStatuses = bytes.Skip(1).Select(status => (PortStatus)status).ToArray();
        }

        public override string ToString()
        {
            return string.Join(",", PortsStatuses);
        }
    }
    class StatusResponseBody : MessageBody
    {
        public Result Result { get; protected set; }

        public StatusResponseBody(Result result)
        {
            Length = 1;

            Result = result;
        }

        public override byte[] ToBytes()
        {
            return new byte[] { (byte)Result };
        }

        public override string ToString()
        {
            return Result.ToString();
        }
    }

    class SendSmsRequestBody : MessageBody
    {
        public byte Port { get; set; }
        public string Number { get; set; }
        public string Message { get; set; }

        protected short ContentLength { get; set; }

        public SendSmsRequestBody(byte port, string number, string message)
        {
            Port = port;
            Number = number;
            Message = message;

            ContentLength = (short)Encoding.Unicode.GetBytes(Message).Length;
            if (ContentLength > 1340)
                throw new NotSupportedException("Messages encoded to Unicode which more than 1340 bytes not supported");


            Length = 30 + ContentLength; //30 byte = port + number  + message
        }

        public override byte[] ToBytes()
        {
            byte[] bytes = new byte[Length];

            bytes[0] = Port;
            bytes[1] = 1; //always Unicode
            bytes[2] = 0; //always SMS
            bytes[3] = 1; //always 1 number to sms

            byte[] numberBytes = Encoding.ASCII.GetBytes(Number).Concat(new byte[24 - Number.Length]).ToArray();
            Array.Copy(numberBytes, 0, bytes, 4, 24);

            byte[] messageLengthBytes = BitConverter.GetBytes((short)ContentLength).Reverse().ToArray();
            Array.Copy(messageLengthBytes, 0, bytes, 28, 2);

            byte[] messageBytes = Encoding.BigEndianUnicode.GetBytes(Message);
            Array.Copy(messageBytes, 0, bytes, 30, messageBytes.Length);

            return bytes;
        }

        public override string ToString()
        {
            return "";
        }
    }
    class SendSmsResponseBody : MessageBody
    {
        public SendSmsResult Result { get; protected set; }

        public SendSmsResponseBody(byte[] bytes)
        {
            Result = (SendSmsResult)bytes[0];
        }

        public override string ToString()
        {
            return Result.ToString();
        }
    }

    class SendSmsResultRequestBody : MessageBody
    {
        public byte CountOfNumbers { get; set; }
        public string Number { get; set; }
        public byte Port { get; set; }
        public SendSmsResult Result { get; set; }
        public byte CountOfSlices { get; set; }
        public byte SucceededSlices { get; set; }

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

            Result = (SendSmsResult)readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            CountOfSlices = readingBytes.Take(1).First();
            readingBytes = readingBytes.Skip(1);

            SucceededSlices = readingBytes.Take(1).First();
        }

        public override string ToString()
        {
            return string.Format("Number: {0}; Result: {1}", Number.Trim(), Result);
        }
    }
    class SendSmsResultResponseBody : MessageBody
    {
        public Result Result { get; protected set; }

        public SendSmsResultResponseBody(Result result)
        {
            Length = 1;

            Result = result;
        }

        public override byte[] ToBytes()
        {
            return new byte[] { (byte)Result };
        }

        public override string ToString()
        {
            return Result.ToString();
        }
    }
}
