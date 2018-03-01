using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using EncCommsPacketsDll;

namespace EncComms_Server
{
    class TcpIO
    {
        TcpClient _client;
        ILoggable Log;
        private static ManualResetEvent readyToSend = new ManualResetEvent(true);
        private static ManualResetEvent readyToReceive = new ManualResetEvent(true);
        const byte sizeOfPacketLength = 4;
        IVisitor higherPower;
        public bool higherPowerSet { get; private set; }
        public TcpIO(TcpClient client, ILoggable Log)
        {
            _client = client;
            higherPowerSet = false;
            this.Log = Log;
        }

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
                Log.AddToLog(bytesSent + " Bytes Sent to : " + _client.Client.RemoteEndPoint.ToString());
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


            Byte[] sizeBuffer = new Byte[bytesToRead];// Possibility of making one large array thats overwritten?
            _client.Client.BeginReceive(sizeBuffer, 0, bytesToRead, SocketFlags.None, new AsyncCallback(receivePacketCallBack), sizeBuffer);
        }
        private void receivePacketCallBack(IAsyncResult ar)
        {
            int bytesReceived = _client.Client.EndReceive(ar);
            readyToReceive.Set();
            Console.WriteLine(bytesReceived + " Bytes read for packet");
            Byte[] receivedData = (Byte[])ar.AsyncState;
            Console.WriteLine("Packet Size: " + receivedData.Length);
            Log.AddToLog(bytesReceived + " Bytes Received From : " + _client.Client.RemoteEndPoint.ToString());
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

        public void setHigherPower(IVisitor c)
        {
            if (!higherPowerSet)
            {
                higherPower = c;
                higherPowerSet = true;
            }
        }
    }
}
