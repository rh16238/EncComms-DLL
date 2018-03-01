using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace EncCommsPacketsDll
{
    [Serializable]
    public abstract class Packet 
    {
        public virtual Byte[] toTransmittable()
        {
            byte[] bytes;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, this);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        public abstract void Accept(IVisitor Visitor);

        public static Packet deserialiseData(Byte[] data)
        {
            Packet p;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {

                p = (Packet)formatter.Deserialize(stream);
            }
            
            return p;
        }
    }

    public interface IVisitor
    {
        void HandlePacket(Packet p);
        void Visit(LogIDPacket packet);
        void Visit(EncryptionHandledFlagPacket packet);
        void Visit(RSAParamPacket packet);
        void Visit(AESParamPacket packet);
        void Visit(AESEncryptedPacket packet);
        void Visit(RSAEncryptedPacket packet);
    }
}
