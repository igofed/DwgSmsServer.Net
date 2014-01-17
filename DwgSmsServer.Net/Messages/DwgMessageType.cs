namespace DwgSmsServerNet.Messages
{
    enum DwgMessageType : byte
    {
        KeepAlive = 0x00,

        SendSmsRequest = 0x01,
        SendSmsResponse = 0x02,

        SendSmsResultRequest = 0x03,
        SendSmsResultResponse = 0x04,

        ReceiveSmsMessageRequest = 0x05,
        ReceiveSmsMessageResponse = 0x06,

        StatusRequest = 0x07,
        StatusResponse = 0x08,

        SendUssdRequest = 0x09,
        SendUssdResponse = 0x0A,

        ReceiveUssdMessageRequest = 0x0B,
        ReceiveUssdMessageResponse = 0x0C,

        CsqRssiRequest = 0x0D,
        CsqRssiResponse = 0x0E,

        AuthenticationRequest = 0x0F,
        AuthenticationResponse = 0x10,

        ReceiveSmsReceiptRequest = 0x11,
        ReceiveSmsReceiptResponse = 0x12
    }
}
