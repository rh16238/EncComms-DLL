using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EncCommsPacketsDll
{
    public interface ITransmitter
    {
        void TransmitPacket(Packet p);
        void EncryptionCompleteCallback();
    }

    public class EncryptionHandler
    {
        public bool RSAEncryptionActive { get; private set; }
        public bool AESEncryptionActive { get; private set; }
        public bool Master { get; private set; }
        byte[] aesKey;

        RSAParameters localParamaters;
        RSAParameters remoteParamaters;

        ITransmitter IO;

        public EncryptionHandler(bool Master, ITransmitter IO)
        {
            this.Master = Master;
            RSAEncryptionActive = false;
            AESEncryptionActive = false;
            localParamaters = RsaEncrytionHandler.returnKeys();
            this.IO = IO;
            if (Master)
            {
                aesKey = AesEncryptionHandler.GenerateKey(256);
            }
        }
        public void initiateEncryptionHandshake()
        {
            if (Master)
            {
                IO.TransmitPacket(new RSAParamPacket(localParamaters));
            }
        }
        public void handleRSA_Packet(RSAParamPacket p)
        {
            remoteParamaters = p.PublicKey;
            RSAEncryptionActive = true;
            if (Master)
            {
                IO.TransmitPacket(new AESParamPacket(aesKey, localParamaters, remoteParamaters));
            }
            else
            {
                IO.TransmitPacket(new RSAParamPacket(localParamaters));
            }
            Console.WriteLine("Received RSA PARAMS");
        }

        public void handleAES_Packet(AESParamPacket p)
        {
            if (Master)
            {
                //This should not happen...
            }
            else if (p.CheckSignature(remoteParamaters)) 
            {

                aesKey = p.DecryptKey(localParamaters);
                AESEncryptionActive = true;
                Console.WriteLine("Received AES PARAMS");
                IO.TransmitPacket(new EncryptionHandledFlagPacket(EncryptionHandledFlagPacket.EncryptionStatus.AESEncryption));
                IO.EncryptionCompleteCallback();
            }
        }
        public void handleFlagPacket(EncryptionHandledFlagPacket p)
        {
            if (p.Status == EncryptionHandledFlagPacket.EncryptionStatus.NoEncryption)
            {
                IO.TransmitPacket(new RSAParamPacket(localParamaters));
            }
            else if (p.Status == EncryptionHandledFlagPacket.EncryptionStatus.RSAEncryption)
            {
                if (Master)
                {
                    IO.TransmitPacket(new AESParamPacket(aesKey, localParamaters, remoteParamaters));
                }
                else
                {
                    IO.TransmitPacket(new RSAParamPacket(localParamaters));
                }
            }
            else if (p.Status == EncryptionHandledFlagPacket.EncryptionStatus.AESEncryption)
            {
                if (Master)
                {
                    AESEncryptionActive = true;
                    Console.WriteLine("Received Awk Packet");
                    IO.EncryptionCompleteCallback();
                }

            }
        }

        public Packet encryptPacket(Packet p)
        {
            Packet returnPacket = p;
            if (AESEncryptionActive)
            {
                returnPacket = new AESEncryptedPacket(p, aesKey);
            }
            //  else if (RSAEncryptionActive)
            //{
            //      returnPacket = new RSAEncryptedPacket(remoteParamaters, localParamaters, p);
            // }
            return returnPacket;
        }

        public Packet decryptAES(AESEncryptedPacket p)
        {
            return p.DecryptPayload(aesKey);
        }

        public Packet decryptRSA(RSAEncryptedPacket p)
        {
            if (p.CheckSignature(remoteParamaters))
            {
                return p.DecryptPayload(localParamaters);
            }
            else { return null; }
        }

    }
}
