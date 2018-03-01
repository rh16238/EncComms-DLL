using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EncCommsPacketsDll;
using System.Collections.Concurrent;
using System.Threading;
namespace encComms_Client
{
    class Engine : IVisitor
    {
        ConcurrentBag<Server> connectedServers;
        MainUIForm uiForm;

        public Engine(MainUIForm ui)
        {
            uiForm = ui;
            connectedServers = new ConcurrentBag<Server>();
        }
        private void addConnectedServer(Server server)
        {
            connectedServers.Add(server);
        }
        public void connectToServer(String Ip, Int32 port)
        {
            Server s = new Server(Ip, port, new Action<Server>(addConnectedServer), this);
        }

        public void broadCastIDPacket(String p)
        {
            while (connectedServers.Count == 0)
            {
                Thread.Sleep(1000);
            }
            foreach (Server s in connectedServers)
            {
                s.transmitLOGID(p);
            }
        }

        public void HandlePacket(Packet p)
        {
            p.Accept(this);
        }


        #region unexpectedPackets
        public void Visit(LogIDPacket packet)
        {
            //Disregard Packet, Can't handle.
        }
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
