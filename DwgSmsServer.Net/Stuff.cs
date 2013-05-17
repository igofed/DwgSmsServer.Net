using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DwgSmsServerNet
{
    /// <summary>
    /// Delegate for informing about DWG SMS server state changed
    /// </summary>
    /// <param name="state">New state of Dwg SMS server</param>
    public delegate void DwgStateChangedDelegate(DwgSmsServerState state);
    /// <summary>
    /// Delegate for informing about sent SMS result
    /// </summary>
    /// <param name="port">Port of DWG, which sent SMS</param>
    /// <param name="number">Phone number of SMS reciever</param>
    /// <param name="result">Sending result</param>
    /// <param name="totalSlices">Count of SMS slices</param>
    /// <param name="succededSlices">Count of succeeded SMS slices</param>
    public delegate void DwgSmsSendingResultDelegate(int port, string number, DwgSendSmsResult result, int totalSlices, int succededSlices);
    /// <summary>
    /// Delegate for informing about DWG errors
    /// </summary>
    /// <param name="error">Error text</param>
    public delegate void DwgErrorDelegate(string error);

    /// <summary>
    /// DWG SMS server state
    /// </summary>
    public enum DwgSmsServerState
    {
        /// <summary>
        /// DWG listener not started
        /// </summary>
        Disconnected,
        /// <summary>
        /// DWG listener started, waiting for DWG to establish socket connection
        /// </summary>
        WaitingDwg,
        /// <summary>
        /// DWG established connection, waiting for welcome packet exhange
        /// </summary>
        Connecting,
        /// <summary>
        /// DWG connected
        /// </summary>
        Connected
    }
    /// <summary>
    /// Result of sending SMS
    /// </summary>
    public enum DwgSendSmsResult : byte
    {
        Succeed = 0,
        Fail = 1,
        Timeout = 2,
        BadRequest = 3,
        PortUnavailable = 4,
        PartialSucceed = 5,
        OtherRrror = 255
    }
    /// <summary>
    /// DWG port status
    /// </summary>
    public enum DwgPortStatus : byte
    {
        Works = 0,
        NoSim = 1,
        NotRegistered = 2,
        Unavailable = 3
    }
}
