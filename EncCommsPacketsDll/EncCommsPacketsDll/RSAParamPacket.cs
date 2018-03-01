using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
namespace EncCommsPacketsDll
{
     [Serializable]
   public class RSAParamPacket : Packet
    {
       public RSAParameters PublicKey { get; private set; }

       
       public RSAParamPacket(RSAParameters localKey)
       {
           PublicKey = localKey;
       }
       public override void Accept(IVisitor Visitor)
       {
           Visitor.Visit(this);
       }
    }
}
