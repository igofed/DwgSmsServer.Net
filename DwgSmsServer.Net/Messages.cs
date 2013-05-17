using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet.Messages
{
    class DwgMessage
    {
        public DwgMessageHeader Header { get; private set; }
        public DwgMessageBody Body { get; private set; }

        public DwgMessage(DwgMessageBody body, byte[] mac)
        {
            DwgMessageType? type = null;

            if (body is AuthenticationRequestBody)
            {
                type = DwgMessageType.AuthenticationRequest;
            }
            else if (body is AuthenticationResponseBody)
            {
                type = DwgMessageType.AuthenticationResponse;
            }
            else if (body is CsqRssiRequestBody)
            {
                type = DwgMessageType.CsqRssiRequest;
            }
            else if (body is CsqRssiResponseBody)
            {
                type = DwgMessageType.CsqRssiResponse;
            }
            else if (body is StatusRequestBody)
            {
                type = DwgMessageType.StatusRequest;
            }
            else if (body is StatusResponseBody)
            {
                type = DwgMessageType.StatusResponse;
            }
            else if (body is SendSmsRequestBody)
            {
                type = DwgMessageType.SendSmsRequest;
            }
            else if (body is SendSmsResponseBody)
            {
                type = DwgMessageType.SendSmsResponse;
            }
            else if (body is SendSmsResultRequestBody)
            {
                type = DwgMessageType.SendSmsResultRequest;
            }
            else if (body is SendSmsResultResponseBody)
            {
                type = DwgMessageType.SendSmsResultResponse;
            }
            else if (body is SendUssdRequestBody)
            {
                type = DwgMessageType.SendUssdRequest;
            }
            else if (body is SendUssdResponseBody)
            {
                type = DwgMessageType.SendUssdResponse;
            }

            if (!type.HasValue)
                throw new NotSupportedException("This type of messages not supported");

            Header = new DwgMessageHeader(type.Value, new byte[] { 0, 0, 0, 0, 0, 0 }, body.Length); ;

            Body = body;
        }

        public DwgMessage(byte[] bytes)
        {
            var readingBytes = bytes.AsEnumerable();

            Header = new DwgMessageHeader(readingBytes.Take(24).ToArray());
            readingBytes = readingBytes.Skip(24);

            var bodyBytes = readingBytes.Take(Header.BodyLength).ToArray();

            switch (Header.Type)
            {
                case DwgMessageType.AuthenticationRequest:
                    Body = new AuthenticationRequestBody(bodyBytes);
                    break;
                case DwgMessageType.StatusRequest:
                    Body = new StatusRequestBody(bodyBytes);
                    break;
                case DwgMessageType.CsqRssiRequest:
                    Body = new CsqRssiRequestBody(bodyBytes);
                    break;
                case DwgMessageType.SendSmsResponse:
                    Body = new SendSmsResponseBody(bodyBytes);
                    break;
                case DwgMessageType.SendSmsResultRequest:
                    Body = new SendSmsResultRequestBody(bodyBytes);
                    break;
                case DwgMessageType.KeepAlive:
                    Body = new KeepAliveBody();
                    break;
                case DwgMessageType.SendUssdResponse:
                    Body = new SendUssdResponseBody(bodyBytes);
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
            return String.Format("{0}: {1}", Header, Body);
        }
    }

    class DwgMessageHeader
    {
        public DwgMessageType Type { get; private set; }
        public byte[] Mac { get; private set; }
        public int BodyLength { get; private set; }

        public DwgMessageHeader(DwgMessageType type, byte[] mac, int bodyLength)
        {
            Type = type;
            Mac = mac;
            BodyLength = bodyLength;
        }

        public DwgMessageHeader(byte[] bytes)
        {
            byte[] tmp = new byte[2];
            Array.Copy(bytes, 20, tmp, 0, 2);
            Type = (DwgMessageType)BitConverter.ToInt16(tmp.Reverse().ToArray(), 0);

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

    abstract class DwgMessageBody
    {
        public int Length { get; protected set; }

        public virtual byte[] ToBytes()
        {
            throw new NotSupportedException();
        }
    }
    enum DwgMessageType : byte
    {
        KeepAlive = 0x00,

        SendSmsRequest = 0x01,
        SendSmsResponse = 0x02,

        SendSmsResultRequest = 0x03,
        SendSmsResultResponse = 0x04,

        StatusRequest = 0x07,
        StatusResponse = 0x08,

        SendUssdRequest = 0x09,
        SendUssdResponse = 0x0A,
        
        ReceiveUssdMessageRequest = 0x0B,
        ReceiveUssdMessageResponse = 0x0C,

        CsqRssiRequest = 0x0D,
        CsqRssiResponse = 0x0E,

        AuthenticationRequest = 0x0F,
        AuthenticationResponse = 0x10
    }
    enum Result : byte
    {
        Succeed = 0,
        Fail = 1
    }

    class AuthenticationRequestBody : DwgMessageBody
    {
        public AuthenticationRequestBody(byte[] bytes)
        {
            User = Encoding.ASCII.GetString(bytes.Take(16).ToArray()).Trim('\0');
            Password = Encoding.ASCII.GetString(bytes.Skip(16).Take(16).ToArray()).Trim('\0');
        }

        public string User { get; private set; }
        public string Password { get; private set; }

        public override string ToString()
        {
            return string.Format("User:{0}; Password:{1}", User, Password);
        }
    }
    class AuthenticationResponseBody : DwgMessageBody
    {
        public Result Result { get; private set; }

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

    class SendSmsRequestBody : DwgMessageBody
    {
        public byte Port { get; private set; }
        public string Number { get; private set; }
        public string Message { get; private set; }
        public short ContentLength { get; private set; }

        public SendSmsRequestBody(byte port, string number, string message)
        {
            Port = port;
            Number = number;
            Message = message;

            ContentLength = (short)Encoding.BigEndianUnicode.GetByteCount(message);

            Length = 30 + ContentLength; //30 byte = port + number  + message
        }

        public override byte[] ToBytes()
        {
            //Port number
            //encding = always Unicode
            //message type = always SMS
            //ncountofnumbers = always 1 number to sms
            byte[] infoBytes = { Port, 1, 0, 1 };
            var numberBytes = Encoding.ASCII.GetBytes(Number).Concat(new byte[24 - Number.Length]);
            var messageLengthBytes = BitConverter.GetBytes((short)ContentLength).Reverse();
            var messageBytes = Encoding.BigEndianUnicode.GetBytes(Message);

            return infoBytes.Concat(numberBytes).Concat(messageLengthBytes).Concat(messageBytes).ToArray();
        }

        public override string ToString()
        {
            return string.Format("Number: {0}; Message: {1}", Number, Message);
        }
    }
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
    class SendSmsResultResponseBody : DwgMessageBody
    {
        public Result Result { get; private set; }

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
    class StatusResponseBody : DwgMessageBody
    {
        public Result Result { get; private set; }

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
    class CsqRssiResponseBody : DwgMessageBody
    {
        public Result Result { get; private set; }

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

    class SendUssdRequestBody : DwgMessageBody
    {
        public byte Port { get; private set; }
        public DwgUssdType Type { get; private set; }
        public short ContentLength { get; private set; }
        public string Content { get; private set; }

        public SendUssdRequestBody(byte port, DwgUssdType type, string content)
        {
            Port = port;
            Content = content;
            Type = type;

            Length = 4 + Encoding.ASCII.GetByteCount(content);
        }

        public override byte[] ToBytes()
        {
            byte[] info = { Port, (byte)Type };
            var contentLengthBytes = BitConverter.GetBytes(ContentLength).Reverse();
            var contentBytes = Encoding.ASCII.GetBytes(Content);

            return info.Concat(contentLengthBytes).Concat(contentBytes).ToArray();
        }
    }
    class SendUssdResponseBody : DwgMessageBody
    {
        public DwgSendUssdResult Result { get; private set; }

        public SendUssdResponseBody(byte[] bytes)
        {
            Length = 1;

            Result = (DwgSendUssdResult)bytes.First();
        }
    }

    class KeepAliveBody : DwgMessageBody
    {
        public override byte[] ToBytes()
        {
            return new byte[0];
        }
    }
}
