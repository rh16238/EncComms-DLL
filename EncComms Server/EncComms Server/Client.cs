using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EncCommsPacketsDll;
namespace EncComms_Server
{
    class Client : IVisitor , ITransmitter
    {
      
        TcpIO connection;
        IVisitor higherAuthority;
        ILoggable Log;
        public string id { get; private set; }
        EncryptionHandler encryptionHandler;
        public Client(TcpIO connection, IVisitor s, ILoggable Log)
        {
            id = "Not Set";
            this.connection = connection;
            higherAuthority = s;
            connection.setHigherPower(this);
            this.Log = Log;
            encryptionHandler = new EncryptionHandler(false, this);
            connection.beginReceivePacketSize();
        }

        public void Visit(LogIDPacket packet)
        {
            this.id = packet.Identity;
            higherAuthority.HandlePacket((Packet)packet);
        }

        public void HandlePacket(Packet p)
        {
            p.Accept(this);
        }


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
            Log.AddToLog("Received AES Key");
        }

        public void Visit(RSAParamPacket packet)
        {
            encryptionHandler.handleRSA_Packet(packet);
            Log.AddToLog("Received RSA Key");
        }

        public void Visit(EncryptionHandledFlagPacket packet)
        {
            encryptionHandler.handleFlagPacket(packet);
        }

        public void EncryptionCompleteCallback()
        {
            //Dont want to do anything here
        }

        public void TransmitPacket(Packet p)
        {
            Packet packetToTransmit = encryptionHandler.encryptPacket(p);
            connection.sendPacket(packetToTransmit.toTransmittable());
        }

        public bool encryptionReady()
        {
            return encryptionHandler.AESEncryptionActive;
        }
    }
}
