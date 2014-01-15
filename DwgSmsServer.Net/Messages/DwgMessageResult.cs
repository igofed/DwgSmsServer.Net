namespace DwgSmsServerNet.Messages
{
    /// <summary>
    /// Result of message processing
    /// </summary>
    enum DwgMessageResult : byte
    {
        /// <summary>
        /// Processing successed
        /// </summary>
        Succeed = 0,
        /// <summary>
        /// Processing failed
        /// </summary>
        Fail = 1
    }
}
