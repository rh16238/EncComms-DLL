using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace EncCommsPacketsDll
{
    [Serializable]
    public class LogIDPacket : Packet
    {
        public string Identity { get; private set; }

        public LogIDPacket(String id)
        {
            this.Identity = id;
        }

        public override void Accept(IVisitor Visitor)
        {
            Visitor.Visit(this);
        }
    }
}
