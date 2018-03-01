using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;

namespace EncCommsPacketsDll
{
    public class RSAEncryptedPacket : Packet
    {
        public Byte[] EncryptedPacket { get; private set; }
        public Byte[] Signature { get; private set; }

        public RSAEncryptedPacket(RSAParameters remoteParam, RSAParameters localParam, Packet p)
        {
            EncryptedPacket = RsaEncrytionHandler.encryptData(remoteParam, p.toTransmittable());
            Signature = RsaEncrytionHandler.createSignature(EncryptedPacket, localParam);
        }
        public override void Accept(IVisitor Visitor)
        {
            Visitor.Visit(this);
        }

        public bool CheckSignature(RSAParameters remoteParam)
        {
            return RsaEncrytionHandler.CheckSignature(remoteParam, EncryptedPacket, Signature);
        }

        public Packet DecryptPayload(RSAParameters localParam)
        {

            byte[] rawData = RsaEncrytionHandler.DecryptData(localParam, EncryptedPacket);
            return Packet.deserialiseData(rawData);

           
        }
    }
}
