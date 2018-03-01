using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EncCommsPacketsDll
{
    [Serializable]
   public class AESEncryptedPacket: Packet
    {

        public Byte[] IV { get; private set; }

        public Byte[] EncryptedPacket { get; private set; }

        public AESEncryptedPacket(Packet p, Byte[] Key)
        {
            AesEncryptionData data = AesEncryptionHandler.EncryptBytes_Aes(p.toTransmittable(), Key);
            IV = data.IV;
            EncryptedPacket = data.EncryptedData;
        }

        public override void Accept(IVisitor Visitor)
        {
            Visitor.Visit(this);
        }

        public Packet DecryptPayload(byte[] Key)
        {
            byte[] rawData = AesEncryptionHandler.DecryptBytes_Aes(EncryptedPacket, Key, IV);
            return Packet.deserialiseData(rawData);

        }
    }
}
