using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using EncCommsPacketsDll;
namespace encComms_Client
{
    class Server : IVisitor, ITransmitter
    {
        Tcp_Connection connection;
        Action<Server> listServerAsConnected;
        IVisitor higherPower;
        EncryptionHandler encryptionHandler;
        public Server(String ip, Int32 port, Action<Server> listServerAsConnected, IVisitor higherPower)
        {
            this.higherPower = higherPower;
            this.listServerAsConnected = listServerAsConnected;
            
            connection = new Tcp_Connection(this, new Action(connectionCallBack));
            connection.Connect(ip, port);
        }


        public void HandlePacket(Packet p)
        {
            p.Accept(this);
        }

        private void connectionCallBack()
        {
            if (connection.connectionSuccess)
            {

                encryptionHandler = new EncryptionHandler(true, this);
                encryptionHandler.initiateEncryptionHandshake();
            }
        }
        #region debug
        public void transmitLOGID(String id)
        {
            LogIDPacket p = new LogIDPacket(id);
            connection.sendPacket(p.toTransmittable());
        }
        #endregion


        public void Visit(RSAEncryptedPacket packet)
        {
            Packet p = encryptionHandler.decryptRSA(packet);
            p.Accept(this);
        }

        public void Visit(AESEncryptedPacket packet)
        {
            Packet p = encryptionHandler.decryptAES(packet);
            p.Accept(this);
        }

        public void Visit(AESParamPacket packet)
        {
            encryptionHandler.handleAES_Packet(packet);
        }

        public void Visit(RSAParamPacket packet)
        {
            encryptionHandler.handleRSA_Packet(packet);
        }

        public void Visit(EncryptionHandledFlagPacket packet)
        {
            encryptionHandler.handleFlagPacket(packet);
        }

        #region refusedPackets
        public void Visit(LogIDPacket packet)
        {
            //Disregard Packet, nonsense.
        }
        #endregion
        
        public void EncryptionCompleteCallback()
        {
                listServerAsConnected(this);
        }

        public void TransmitPacket(Packet p)
        {
            Packet packetToTransmit = encryptionHandler.encryptPacket(p);
            connection.sendPacket(packetToTransmit.toTransmittable());
        }
    }
}
