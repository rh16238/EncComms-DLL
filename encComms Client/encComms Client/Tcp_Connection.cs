using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using EncCommsPacketsDll;
namespace encComms_Client
{
    class Tcp_Connection
    {
        TcpClient _client;
        IPAddress[] parsedIP;
        private static ManualResetEvent dnsDone = new ManualResetEvent(false);
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent readyToSend =new ManualResetEvent(true);
        private static ManualResetEvent readyToReceive =new ManualResetEvent(false);
        public bool connectionSuccess { get; private set; }
        Action  connectionCompletedCallBack;
        IVisitor higherPower;
        public Tcp_Connection(IVisitor higherPower, Action connectionCallBack)
        {
            this.higherPower = higherPower;
            this.connectionCompletedCallBack = connectionCallBack;
            _client = new TcpClient();
            connectionSuccess = false;
        }
        #region connecting
        public bool Connect(String ipAddress, int port)
        {
            try
            {
                Dns.BeginGetHostAddresses(ipAddress, new AsyncCallback(getHostCallBack), null);
            
            dnsDone.WaitOne();

            _client.BeginConnect(parsedIP, port, new AsyncCallback(connectionCallBack), _client);
            }
          
            catch (Exception) { return false; }
            connectDone.WaitOne();
            readyToReceive.Set();
            if (connectionSuccess) { Console.WriteLine("Connected"); }
            return connectionSuccess;
        }

        private void getHostCallBack(IAsyncResult ar)
        {
            parsedIP = Dns.EndGetHostAddresses(ar);
            dnsDone.Set();
        }

        private void connectionCallBack(IAsyncResult ar)
        {
            _client = (TcpClient)ar.AsyncState;
            try
            {
                _client.EndConnect(ar);
                connectionSuccess = true;
            }
            catch (SocketException)
            {

                connectionSuccess = false;
            }
            connectDone.Set();
            connectionCompletedCallBack();
            beginReceivePacketSize();
        }
        #endregion
        #region sending
        public void sendPacket(Byte[] dataToSend)
        {
            readyToSend.WaitOne();
            readyToSend.Reset();
            Int32 arraySize = dataToSend.Length;
            Byte[] arraySizeBytes = BitConverter.GetBytes(arraySize);
            arraySizeBytes = arraySizeBytes.Concat(dataToSend).ToArray();
            try
            {
                if (arraySize == 0) { throw new ArgumentOutOfRangeException("PacketSize", "0", "BS Size must be larger than 0"); }
                _client.Client.BeginSend(arraySizeBytes, 0, arraySizeBytes.Length, SocketFlags.None, new AsyncCallback(sendPacketCallBack), null);
            }
            catch (SocketException ex) { Console.WriteLine("BS Error, Socket Exception: " + ex.ErrorCode + " : " + ex.Message); readyToSend.Set(); }
        }

        private void sendPacketCallBack(IAsyncResult ar)
        {
            try
            {

                int bytesSent = _client.Client.EndSend(ar);
                Console.WriteLine(bytesSent + " Bytes Sent");
            }
            catch (SocketException ex) { Console.WriteLine("ES Error, Socket Exception: " + ex.ErrorCode + " : " + ex.Message); readyToSend.Set(); }
            readyToSend.Set();

        }
        #endregion
        #region receiving
        public void beginReceivePacketSize()
        {
            readyToReceive.WaitOne();
            readyToReceive.Reset();

            Byte[] sizeBuffer = new Byte[4];
            _client.Client.BeginReceive(sizeBuffer, 0, 4, SocketFlags.None, new AsyncCallback(receivePacketSizeCallBack), sizeBuffer);
        }
        private void receivePacketSizeCallBack(IAsyncResult ar)
        {
            int bytesReceived = _client.Client.EndReceive(ar);
            Console.WriteLine(bytesReceived + " Bytes read for size");
            Byte[] receivedData = (Byte[])ar.AsyncState;
            int bytesToRead = BitConverter.ToInt32(receivedData, 0);
            Console.WriteLine("Packet Size: " + bytesToRead);

            if (bytesToRead != 0)//Bytes to read
            {
                Byte[] sizeBuffer = new Byte[bytesToRead];// Possibility of making one large array thats overwritten?
                _client.Client.BeginReceive(sizeBuffer, 0, bytesToRead, SocketFlags.None, new AsyncCallback(receivePacketCallBack), sizeBuffer);
            }
            else//Empty Packet wtf?
            {
                Byte[] sizeBuffer = new Byte[4];
                _client.Client.BeginReceive(sizeBuffer, 0, 4, SocketFlags.None, new AsyncCallback(receivePacketSizeCallBack), sizeBuffer);
            }
        }
        private void receivePacketCallBack(IAsyncResult ar)
        {
            int bytesReceived = _client.Client.EndReceive(ar);
            readyToReceive.Set();
            Console.WriteLine(bytesReceived + " Bytes read for packet");
            Byte[] receivedData = (Byte[])ar.AsyncState;
            Console.WriteLine("Packet Size: " + receivedData.Length);

            beginReceivePacketSize();

            Packet p = Packet.deserialiseData(receivedData);
            p.Accept(higherPower);
        }
        #endregion

        #region Closing
        private void closeConnection()
        {

            _client.Close();
        }
        #endregion
    }
}
