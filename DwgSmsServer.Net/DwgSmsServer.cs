using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace DwgSmsServer.Net
{
    public class DwgSmsServer
    {
        public int Port { get; protected set; }
        public string User { get; protected set; }
        public string Password { get; protected set; }

        public DwgSmsServerStatus Status { get; set; }

        private Thread _listenerThread = null;
        private Socket _listenSocket = null;

        public DwgSmsServer(int port, string user, string password)
        {
            Port = port;
            User = user;
            Password = password;

            Status = DwgSmsServerStatus.Disconnected;
        }

        public void Start()
        {
            if (Status != DwgSmsServerStatus.Disconnected)
                throw new InvalidOperationException("Can't start already started server");

            Status = DwgSmsServerStatus.WaitingDwg;

            _listenerThread = new Thread(WorkingThread);
            _listenerThread.Start();
        }

        public void Stop()
        {
            if(Status == DwgSmsServerStatus.Disconnected)
                throw new InvalidOperationException("Can't stop not running server");

            _listenerThread.Abort();
        }

        private void WorkingThread()
        {
            try
            {
                //here listen code
            }
            catch (ThreadAbortException)
            {
                _listenSocket.Close();
            }
        }
    }

    public enum DwgSmsServerStatus
    {
        Disconnected,
        WaitingDwg,
        Connection,
        Connected       
    }
}
