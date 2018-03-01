using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EncCommsPacketsDll
{

    [Serializable]
    public class EncryptionHandledFlagPacket : Packet
    {
        public EncryptionStatus Status { get; private set; }
        public enum EncryptionStatus
        {
            NoEncryption = 0,
            RSAEncryption = 1,
            AESEncryption = 2,

        }

        public EncryptionHandledFlagPacket(EncryptionStatus status)
        {
            this.Status = status;
        }
        public override void Accept(IVisitor Visitor)
        {
            Visitor.Visit(this);
        }
    }
}
