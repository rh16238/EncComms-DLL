using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EncCommsPacketsDll;
using System.Collections.Concurrent;
namespace EncComms_Server
{
    interface IClientStore
    {
        void addClient(TcpIO newClient);
    }

    class Server : IClientStore, IVisitor
    {
        ConcurrentBag<Client> ConnectedClients;
        Tcp_Controller listener;
        ServerUI UI;
        ILoggable Log;
        public Server(ServerUI UI, ILoggable Log)
        {
            this.UI = UI;
            this.Log = Log;
            ConnectedClients = new ConcurrentBag<Client>();
            listener = new Tcp_Controller(this, Log);
            listener.BeginListening();

        }
        public void addClient(TcpIO newClient)
        {
            ConnectedClients.Add(new Client(newClient, this, Log));
        }

        public void Visit(LogIDPacket packet)
        {
            List<String> clientNames = new List<String>();
            foreach (Client C in ConnectedClients)
            {
                clientNames.Add(C.id);
            }
            UI.updateClientList(clientNames.ToArray());
        }

        public void HandlePacket(Packet p)
        {
            p.Accept(this);
        }

        #region unexpectedPackets


        public void Visit(RSAEncryptedPacket packet)
        {
            throw new NotImplementedException();
        }

        public void Visit(AESEncryptedPacket packet)
        {
            throw new NotImplementedException();
        }

        public void Visit(AESParamPacket packet)
        {
            throw new NotImplementedException();
        }

        public void Visit(RSAParamPacket packet)
        {
            throw new NotImplementedException();
        }

        public void Visit(EncryptionHandledFlagPacket packet)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
