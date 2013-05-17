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
    public delegate void DwgStateChangedDelegate(DwgSmsServerState state);
    public delegate void DwgSmsSendingResultDelegate(int port, string number, SendSmsResult result, int totalSlices, int succededSlices);
    public delegate void DwgErrorDelegate(string error);

    public class DwgSmsServer
    {
        /// <summary>
        /// Occurs when State of server changed
        /// </summary>
        public event DwgStateChangedDelegate StateChanged = s => { };
        /// <summary>
        /// Occurs on server error
        /// </summary>
        public event DwgErrorDelegate Error = s => { };
        /// <summary>
        /// Occurs on info about sended SMS from Dwg
        /// </summary>
        public event DwgSmsSendingResultDelegate SmsSendingResult = (p, s, r, t, sd) => { };

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
        public ReadOnlyCollection<PortStatus> PortsStatuses { get; set; }

        //all about Dwg listening
        private Thread _listenerThread = null;
        private Socket _listenSocket = null;
        private TcpListener _listener = null;

        //mac address of Dwg
        private byte[] _macAddress = new byte[6];

        //Send sms result
        private AutoResetEvent _sendSmsEvent = new AutoResetEvent(false);
        private SendSmsResult _sendSmsResult;

        /// <summary>
        /// Creating Dwg SMS Server
        /// </summary>
        /// <param name="port">Port to listen Dwg. Should be same in Dwg settings</param>
        /// <param name="user">User name of SMS server. Should be same in Dwg settings</param>
        /// <param name="password">Password of SMS server. Should be same in Dwg settings</param>
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
        /// Start Dwg SMS server
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
        /// Stop Dwg SMS server
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
        /// <param name="port">Dwg port to send from</param>
        /// <param name="number">Phone number</param>
        /// <param name="message">Message to send</param>
        public SendSmsResult SendSms(byte port, string number, string message)
        {
            if (State != DwgSmsServerState.Connected)
                throw new NotSupportedException("Can't send SMS from not connected server");
            if (port < 0 || port > PortsCount || PortsStatuses[port] != PortStatus.Works)
                throw new NotSupportedException("Port should be in \"Works\" status");
            if (Encoding.BigEndianUnicode.GetByteCount(message) > 1340)
                throw new NotSupportedException("Message encoded to Unicode should be less than 1340 bytes");

            SendSmsRequestBody body = new SendSmsRequestBody(port, number, message);
            SendToDwg(body);

            //wait for response from dwg
            _sendSmsEvent.WaitOne();

            return _sendSmsResult;
        }


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

                    if (msg.Header.Type == MessageType.AuthenticationRequest)
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

                    if (msg.Header.Type == MessageType.StatusRequest)
                    {
                        SendToDwg(new StatusResponseBody(Result.Succeed));

                        StatusRequestBody body = msg.Body as StatusRequestBody;
                        
                        PortsCount = body.PortsCount;
                        PortsStatuses = new ReadOnlyCollection<PortStatus>(body.PortsStatuses);
                    }

                    if (msg.Header.Type == MessageType.CsqRssiRequest)
                    {
                        SendToDwg(new CsqRssiResponseBody(Result.Succeed));

                        //if this is first csq rssi message - then from this moment we connected
                        if (State == DwgSmsServerState.Connecting)
                            State = DwgSmsServerState.Connected;
                    }

                    if (msg.Header.Type == MessageType.SendSmsResponse)
                    {
                        SendSmsResponseBody body = msg.Body as SendSmsResponseBody;
                        _sendSmsResult = body.Result;

                        //signal to SendSms method, that response recieved
                        _sendSmsEvent.Set();
                    }

                    if (msg.Header.Type == MessageType.SendSmsResultRequest)
                    {
                        SendToDwg(new SendSmsResultResponseBody(Result.Succeed));

                        SendSmsResultRequestBody body = msg.Body as SendSmsResultRequestBody;
                        SmsSendingResult(body.Port, body.Number, body.Result, body.CountOfSlices, body.SucceededSlices); 
                    }
                }
            }
            catch (ThreadAbortException)
            {
                _listenSocket.Close();
                _listener.Stop();
            }
        }

        private void SendToDwg(MessageBody body)
        {
            DwgMessage msgToSend = new DwgMessage(body, _macAddress);
            _listenSocket.Send(msgToSend.ToBytes());

            Console.WriteLine(msgToSend);
        }
    }

    public enum DwgSmsServerState
    {
        /// <summary>
        /// Dwg listener not started
        /// </summary>
        Disconnected,
        /// <summary>
        /// Dwg listener started, waiting for Dwg to connect
        /// </summary>
        WaitingDwg,
        /// <summary>
        /// Connecting to Dwg
        /// </summary>
        Connecting,
        /// <summary>
        /// Dwg connected
        /// </summary>
        Connected
    }
}
