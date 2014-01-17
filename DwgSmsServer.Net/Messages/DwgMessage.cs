using DwgSmsServerNet.Messages.Bodies;
using System;
using System.Linq;

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
            else if (body is ReceiveUssdMessageRequestBody)
            {
                type = DwgMessageType.ReceiveUssdMessageRequest;
            }
            else if (body is ReceiveUssdMessageResponseBody)
            {
                type = DwgMessageType.ReceiveUssdMessageResponse;
            }
            else if (body is ReceiveSmsMessageRequestBody)
            {
                type = DwgMessageType.ReceiveSmsMessageRequest;
            }
            else if (body is ReceiveSmsMessageResponseBody)
            {
                type = DwgMessageType.ReceiveSmsMessageResponse;
            }
            else if (body is ReceiveSmsReceiptRequestBody)
            {
                type = DwgMessageType.ReceiveSmsReceiptRequest; ;
            }
            else if (body is ReceiveSmsReceiptResponseBody)
            {
                type = DwgMessageType.ReceiveSmsReceiptResponse;
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
                case DwgMessageType.ReceiveUssdMessageRequest:
                    Body = new ReceiveUssdMessageRequestBody(bodyBytes);
                    break;
                case DwgMessageType.ReceiveSmsMessageRequest:
                    Body = new ReceiveSmsMessageRequestBody(bodyBytes);
                    break;
                case DwgMessageType.ReceiveSmsReceiptRequest:
                    Body = new ReceiveSmsReceiptRequestBody(bodyBytes);
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
}
