using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace EncCommsPacketsDll
{
    /// <summary>
    /// Handles encryption via RSA.
    /// </summary>
    static class RsaEncrytionHandler
    {
        /// <summary>
        /// Generates and returns keys for RSA. Used for aes key exchange.
        /// </summary>
        public static RSAParameters returnKeys()
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(3072);//Generates keys with standard size.
            RSAParameters RSAKeyInfo = RSA.ExportParameters(true);// Allows public key to be read as param.

            return RSAKeyInfo;// return info.
        }
        /// <summary>
        /// Decrypts data via RSA Parameters and returns the data. 
        /// </summary>
        /// <param name="RSAP">Paramaters to decrypt via.</param>
        /// <param name="receivedData">Data that needs to be decrypted.</param>
        /// <returns></returns>
        public static byte[] DecryptData(RSAParameters RSAP, byte[] receivedData)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();// Create a service provider
            RSA.ImportParameters(RSAP);//Provide it the parameters

            return RSA.Decrypt(receivedData, false);//Return the decyrpted data.
        }
        /// <summary>
        /// Encrypts data via RSA.
        /// </summary>
        /// <param name="externalKey">Paramaters to encrypt via.</param>
        /// <param name="data">Data to be encrypted.</param>
        /// <returns></returns>
        public static byte[] encryptData(RSAParameters externalKey, byte[] data)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();// Create a service provider
            RSA.ImportParameters(externalKey);//Provide it the parameters

            return RSA.Encrypt(data, false);//Return the encyrpted data.
        }

        public static byte[] createSignature(Byte[] payload, RSAParameters localKey)
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(localKey);
            return RSA.SignData(payload, new SHA256CryptoServiceProvider());
        }

        public static bool CheckSignature(RSAParameters remoteParam, byte[] data, byte[] signature)
        {
            RSACryptoServiceProvider RSAalg = new RSACryptoServiceProvider();

            RSAalg.ImportParameters(remoteParam);
            return RSAalg.VerifyData(data, new SHA256CryptoServiceProvider(), signature);
        }
    }
}
