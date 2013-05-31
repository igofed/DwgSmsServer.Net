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
    /// Delegate for informing about DWG errors
    /// </summary>
    /// <param name="error">Error text</param>
    public delegate void DwgErrorDelegate(string error);
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
    /// Delegate for informing about sent USSD result
    /// </summary>
    /// <param name="port">Port of DWG, which send USSD</param>
    /// <param name="result">Result of USSD from network</param>
    /// <param name="message">Message from executing USSD</param>
    public delegate void DwgUssdSendingResultDelegate(int port, DwgRecieveUssdResult result, string message);    
    /// <summary>
    /// Delegate for informing about receieved SMS 
    /// </summary>
    /// <param name="port">Port receieved SMS</param>
    /// <param name="dateTime">Time of sending SMS</param>
    /// <param name="number">Number, that sent SMS</param>
    /// <param name="message">SMS text</param>
    public delegate void DwgSmsReceivedDelegate(int port, DateTime dateTime, string number, string message);

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
        /// <summary>
        /// SMS sent successfully
        /// </summary>
        Succeed = 0,
        /// <summary>
        /// SMS sent failed
        /// </summary>
        Fail = 1,
        /// <summary>
        /// SMS sent timeout
        /// </summary>
        Timeout = 2,
        /// <summary>
        /// Something wrong with sending SMS request
        /// </summary>
        BadRequest = 3,
        /// <summary>
        /// Specified port is not available for sending SMS
        /// </summary>
        PortUnavailable = 4,
        /// <summary>
        /// Not all parts of SMS successfully sent
        /// </summary>
        PartialSucceed = 5,
        /// <summary>
        /// Some unknown error occurs while sending SMS
        /// </summary>
        OtherError = 255
    }
    /// <summary>
    /// DWG port status
    /// </summary>
    public enum DwgPortStatus : byte
    {
        /// <summary>
        /// Port normally works
        /// </summary>
        Works = 0,
        /// <summary>
        /// No SIM card inserted
        /// </summary>
        NoSim = 1,
        /// <summary>
        /// Not registered on mobile network
        /// </summary>
        NotRegistered = 2,
        /// <summary>
        /// No hardware port
        /// </summary>
        Unavailable = 3
    }
    /// <summary>
    /// DWG USSD type
    /// </summary>
    public enum DwgUssdType
    {
        /// <summary>
        /// Send USSD request
        /// </summary>
        Send = 1,
        /// <summary>
        /// Complete current USSD session
        /// </summary>
        EndSession = 2
    }
    /// <summary>
    /// Result of sending USSD
    /// </summary>
    public enum DwgSendUssdResult : byte
    {
        /// <summary>
        /// USSD sent successfully
        /// </summary>
        Succeed = 0,
        /// <summary>
        /// USSD sent failed
        /// </summary>
        Fail = 1,
        /// <summary>
        /// USSD sent timeout
        /// </summary>
        Timeout = 2,
        /// <summary>
        /// Something wrong with sending USSD request
        /// </summary>
        BadRequest = 3,
        /// <summary>
        /// Specified port is not available for sending SMS
        /// </summary>
        PortUnavailable = 4,
        /// <summary>
        /// Some unknown error occurs while sending SMS
        /// </summary>
        OtherError = 255
    }
    /// <summary>
    /// Result of USSD
    /// </summary>
    public enum DwgRecieveUssdResult : byte
    {
        /// <summary>
        /// No further user action required
        /// </summary>
        NoFurtherUserActionRequired = 0,
        /// <summary>
        /// Further user action required
        /// </summary>
        FurtherUserActionRequired = 1,
        /// <summary>
        /// Ussd terminated by network
        /// </summary>
        UssdTerminatedByNetwork = 2,
        /// <summary>
        /// Operation not supported
        /// </summary>
        OperationNotSupported = 4
    }
}
