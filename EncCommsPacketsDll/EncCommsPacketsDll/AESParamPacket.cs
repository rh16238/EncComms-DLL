using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
namespace EncCommsPacketsDll
{
    [Serializable]
    public class AESParamPacket : Packet
    {
        public Byte[] PrivateKey { get; private set; }
        public Byte[] Signature { get; private set; }
        public AESParamPacket(Byte[] Key, RSAParameters local, RSAParameters remote)
        {
            if (Key == null || Key.Length == 0) { throw new ArgumentOutOfRangeException("Local Key was Null"); }
            PrivateKey = RsaEncrytionHandler.encryptData(remote, Key);
            Signature = RsaEncrytionHandler.createSignature(PrivateKey, local);

        }
        public override void Accept(IVisitor Visitor)
        {
            Visitor.Visit(this);
        }


        public bool CheckSignature(RSAParameters remoteParam)
        {
            return RsaEncrytionHandler.CheckSignature(remoteParam, PrivateKey, Signature);
        }

        public Byte[] DecryptKey(RSAParameters localParam)
        {

            byte[] rawData = RsaEncrytionHandler.DecryptData(localParam, PrivateKey);
            return rawData;


        }
    }
}
