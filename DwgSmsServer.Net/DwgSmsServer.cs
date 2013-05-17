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
        public event Action<DwgSmsServerState> StateChanged = s => { };
        public event Action<string> Error = s => { };

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

        private Thread _listenerThread = null;
        private Socket _listenSocket = null;
        private TcpListener _listener = null;

        private byte[] _macAddress = new byte[6];

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

            State = DwgSmsServerState.Disconnected;
        }

        public void Start()
        {
            if (State != DwgSmsServerState.Disconnected)
                throw new InvalidOperationException("Can't start already started server");

            State = DwgSmsServerState.WaitingDwg;

            _listenerThread = new Thread(WorkingThread);
            _listenerThread.Start();
        }

        public void Stop()
        {
            if (State == DwgSmsServerState.Disconnected)
                throw new InvalidOperationException("Can't stop not running server");

            _listenerThread.Abort();
        }



        private void WorkingThread()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();

                //Listener started - now in WaitingDwg state
                State = DwgSmsServerState.WaitingDwg;
                StateChanged(State);
                

                _listenSocket = _listener.AcceptSocket();
                //Dwg just connected with socket. Now starting to connect
                State = DwgSmsServerState.Connecting;
                StateChanged(State);
                
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
                    }

                    if (msg.Header.Type == MessageType.CsqRssiRequest)
                    {
                        SendToDwg(new CsqRssiResponseBody(Result.Succeed));
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
