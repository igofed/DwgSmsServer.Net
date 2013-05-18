using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using DwgSmsServerNet.Messages;
using System.Collections.ObjectModel;

namespace DwgSmsServerNet
{
    public class DwgSmsServer
    {
        /// <summary>
        /// Occurs when State of server changed
        /// </summary>
        public event DwgStateChangedDelegate StateChanged = s => { };
        /// <summary>
        /// Occurs on server error
        /// </summary>
        public event DwgErrorDelegate Error = e => { };
        /// <summary>
        /// Occurs on info about sent SMS from DWG
        /// </summary>
        public event DwgSmsSendingResultDelegate SmsSendingResult = (p, n, r, t, s) => { };
        /// <summary>
        /// Occurs on info abous sent USSD from DWG
        /// </summary>
        public event DwgUssdSendingResultDelegate UssdSendingResult = (p, r, m) => { };


        /// <summary>
        /// Listen port of SMS Server
        /// </summary>
        public int Port { get; protected set; }
        /// <summary>
        /// User of SMS server
        /// </summary>
        public string User { get; protected set; }
        /// <summary>
        /// Password of SMS server
        /// </summary>
        public string Password { get; protected set; }
        /// <summary>
        /// MAC address of DWG. 
        /// 00-00-00-00-00-00 if not connected.
        /// </summary>
        public string DwgMacAddress { get { return string.Join("-", _macAddress.Select(a => a.ToString("X"))); } }
        /// <summary>
        /// Dwg connectiong status
        /// </summary>
        public DwgSmsServerState State { get { return _state; } protected set { _state = value; StateChanged(_state); } }
        private DwgSmsServerState _state = DwgSmsServerState.Disconnected;
        /// <summary>
        /// Number of Dwg ports
        /// </summary>
        public int PortsCount { get; protected set; }
        /// <summary>
        /// Statuses of ports
        /// </summary>
        public ReadOnlyCollection<DwgPortStatus> PortsStatuses { get; set; }

        //all about DWG listening
        private Thread _listenerThread = null;
        private Socket _listenSocket = null;
        private TcpListener _listener = null;

        //mac address of DWG
        private byte[] _macAddress = new byte[6];

        //Send sms result
        private AutoResetEvent _sendSmsEvent = new AutoResetEvent(false);
        private DwgSendSmsResult _sendSmsResult;

        //Send ussd result
        private AutoResetEvent _sendUssdEvent = new AutoResetEvent(false);
        private DwgSendUssdResult _sendUssdResult;

        /// <summary>
        /// Creating DWG SMS Server
        /// </summary>
        /// <param name="port">Port to listen DWG. Should be same in DWG settings</param>
        /// <param name="user">User name of SMS server. Should be same in DWG settings</param>
        /// <param name="password">Password of SMS server. Should be same in DWG settings</param>
        public DwgSmsServer(int port, string user, string password)
        {
            if (string.IsNullOrEmpty(user))
                throw new ArgumentException("User cannot be empty");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty");

            if (user.Length > 15)
                throw new ArgumentException("User name should be less than 15 symbols");
            if (password.Length > 15)
                throw new ArgumentException("Password should be less than 15 symbols");

            Port = port;
            User = user;
            Password = password;
        }

        /// <summary>
        /// Start DWG SMS server
        /// </summary>
        public void Start()
        {
            if (State != DwgSmsServerState.Disconnected)
                throw new InvalidOperationException("Can't start already started server");

            State = DwgSmsServerState.WaitingDwg;

            _listenerThread = new Thread(WorkingThread);
            _listenerThread.Start();
        }

        /// <summary>
        /// Stop DWG SMS server
        /// </summary>
        public void Stop()
        {
            if (State == DwgSmsServerState.Disconnected)
                throw new InvalidOperationException("Can't stop not running server");

            _listenerThread.Abort();
        }

        /// <summary>
        /// Send SMS message
        /// </summary>
        /// <param name="port">DWG port to send from</param>
        /// <param name="number">Phone number</param>
        /// <param name="message">Message to send</param>
        /// <returns>Result of sending SMS</returns>
        public DwgSendSmsResult SendSms(byte port, string number, string message)
        {
            if (State != DwgSmsServerState.Connected)
                throw new NotSupportedException("Can't send SMS from not connected server");
            if (port < 0 || port > PortsCount || PortsStatuses[port] != DwgPortStatus.Works)
                throw new NotSupportedException("Port should be in \"Works\" status");
            if (Encoding.BigEndianUnicode.GetByteCount(message) > 1340)
                throw new NotSupportedException("Message encoded to Unicode should be less than 1340 bytes");

            SendSmsRequestBody body = new SendSmsRequestBody(port, number, message);
            SendToDwg(body);

            //wait for response from dwg
            _sendSmsEvent.WaitOne();

            return _sendSmsResult;
        }

        /// <summary>
        /// Send USSD request
        /// </summary>
        /// <param name="port">DWG port to send from</param>
        /// <param name="type">USSD type</param>
        /// <param name="ussd">USSD request content</param>
        /// <returns>Result of sending USSD to network</returns>
        public DwgSendUssdResult SendUssd(byte port, DwgUssdType type, string ussd)
        {
            if (State != DwgSmsServerState.Connected)
                throw new NotSupportedException("Can't send SMS from not connected server");
            if (port < 0 || port > PortsCount || PortsStatuses[port] != DwgPortStatus.Works)
                throw new NotSupportedException("Port should be in \"Works\" status");

            SendToDwg(new SendUssdRequestBody(port, type, ussd));

            _sendUssdEvent.WaitOne();

            return _sendUssdResult;
        }

        //main wotk method
        private void WorkingThread()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();

                //Listener started - now in WaitingDwg state
                State = DwgSmsServerState.WaitingDwg;


                _listenSocket = _listener.AcceptSocket();
                //Dwg just connected with socket. Now starting to exchange packets
                State = DwgSmsServerState.Connecting;

                while (true)
                {
                    byte[] buffer = new byte[_listenSocket.ReceiveBufferSize];
                    int size = _listenSocket.Receive(buffer);

                    DwgMessage msg = new DwgMessage(buffer);

                    Console.WriteLine(msg);

                    if (msg.Header.Type == DwgMessageType.AuthenticationRequest)
                    {
                        AuthenticationRequestBody body = msg.Body as AuthenticationRequestBody;
                        if (body.User == User)
                        {
                            if (body.Password == Password)
                            {
                                _macAddress = msg.Header.Mac;

                                SendToDwg(new AuthenticationResponseBody(Result.Succeed));
                            }
                            else
                            {
                                Error(string.Format("Wrong password. Expected: {0}; Recieved: {1}", Password, body.Password));
                                Stop();
                            }
                        }
                        else
                        {
                            Error(string.Format("Wrong user. Expected: {0}; Recieved: {1}", User, body.User));
                            Stop();
                        }
                    }

                    if (msg.Header.Type == DwgMessageType.StatusRequest)
                    {
                        SendToDwg(new StatusResponseBody(Result.Succeed));

                        StatusRequestBody body = msg.Body as StatusRequestBody;

                        PortsCount = body.PortsCount;
                        PortsStatuses = new ReadOnlyCollection<DwgPortStatus>(body.PortsStatuses);
                    }

                    if (msg.Header.Type == DwgMessageType.CsqRssiRequest)
                    {
                        SendToDwg(new CsqRssiResponseBody(Result.Succeed));

                        //if this is first csq rssi message - then from this moment we connected
                        if (State == DwgSmsServerState.Connecting)
                            State = DwgSmsServerState.Connected;
                    }

                    if (msg.Header.Type == DwgMessageType.SendSmsResponse)
                    {
                        SendSmsResponseBody body = msg.Body as SendSmsResponseBody;
                        _sendSmsResult = body.Result;

                        //signal to SendSms method, that response recieved
                        _sendSmsEvent.Set();
                    }

                    if (msg.Header.Type == DwgMessageType.SendSmsResultRequest)
                    {
                        SendToDwg(new SendSmsResultResponseBody(Result.Succeed));

                        SendSmsResultRequestBody body = msg.Body as SendSmsResultRequestBody;
                        SmsSendingResult(body.Port, body.Number, body.Result, body.CountOfSlices, body.SucceededSlices);
                    }

                    if (msg.Header.Type == DwgMessageType.SendUssdResponse)
                    {
                        SendUssdResponseBody body = msg.Body as SendUssdResponseBody;
                        _sendUssdResult = body.Result;
                        
                        //signal to SendUssd method, that response recieved
                        _sendUssdEvent.Set();
                    }

                    if (msg.Header.Type == DwgMessageType.ReceiveUssdMessageRequest)
                    {
                        SendToDwg(new ReceiveUssdMessageResponseBody(Result.Succeed));

                        ReceiveUssdMessageRequestBody body = msg.Body as ReceiveUssdMessageRequestBody;
                        UssdSendingResult(body.Port, body.Status, body.Content);
                    }

                    if (msg.Header.Type == DwgMessageType.ReceiveSmsMessageRequest)
                    {
                        SendToDwg(new ReceiveSmsMessageResponseBody(Result.Succeed));

                        //logic to notify
                    }

                    if (msg.Header.Type == DwgMessageType.KeepAlive)
                    {
                        SendToDwg(new KeepAliveBody());
                    }
                }
            }
            catch (ThreadAbortException)
            {
                _listenSocket.Close();
                _listener.Stop();
            }
        }

        //sending data back to DWG
        private void SendToDwg(DwgMessageBody body)
        {
            DwgMessage msgToSend = new DwgMessage(body, _macAddress);
            _listenSocket.Send(msgToSend.ToBytes());

            Console.WriteLine(msgToSend);
        }
    }
}
