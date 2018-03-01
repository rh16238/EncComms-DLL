using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace EncComms_Server
{
    class Tcp_Controller
    {
        TcpListener listener;
        public static ManualResetEvent tcpClientReadyToConnect = new ManualResetEvent(true);
        public bool endListen { get; private set; }
        IClientStore ClientHandler;
        ILoggable Log;

        public Tcp_Controller(IClientStore ClientHandler, ILoggable Log)
        {
            this.ClientHandler = ClientHandler;
            this.Log = Log;
            listener = new TcpListener(IPAddress.Any, 11000);
            listener.Start(20);
            endListen = false;
        }

        public void BeginListening()
        {
            tcpClientReadyToConnect.WaitOne();
            listener.BeginAcceptTcpClient(new AsyncCallback(getConnectionCallBack), listener);
            Log.AddToLog("Listening on " + listener.LocalEndpoint.ToString());
        }

        private void getConnectionCallBack(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            TcpClient client = listener.EndAcceptTcpClient(ar);
            TcpIO newActor = new TcpIO(client, Log);
            ClientHandler.addClient(newActor);
            Log.AddToLog("New Client Added " + client.Client.RemoteEndPoint.ToString());
            tcpClientReadyToConnect.Set();
            if (!endListen) { BeginListening(); }
        }

        #region Closing
        private void closeConnection()
        {

            listener.Stop();

        }
        #endregion

    }
}
