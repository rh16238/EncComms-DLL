using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace EncCommsPacketsDll
{
    public struct AesEncryptionData
    {
        public Byte[] EncryptedData;
        public Byte[] IV;
    }
    /// <summary>
    /// Handles encrypting AES
    /// </summary>
    static class AesEncryptionHandler
    {
        /// <summary>
        /// Encrypts bytes in AES when given a certain key, and returns encrypted data and IV, used to encrypt files.
        /// </summary>
        /// <param name="Data">Data to encrypt.</param>
        /// <param name="Key">Key to encrypt data with.</param>
        /// <returns>IV and Data concatonated.</returns>
        public static AesEncryptionData EncryptBytes_Aes(byte[] Data, byte[] Key)
        {
            // Check arguments.
            if (Data == null || Data.Length <= 0)
                throw new ArgumentNullException("plainText"); // No data!
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");// No key!
            // Create an Aes object with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.GenerateIV();// generate a suitable IV.
                aesAlg.Padding = PaddingMode.PKCS7;//Set padding mode (must match on decrypt).
                // Create a encrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);// create encryptor via key and IV.


                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {

                        csEncrypt.Write(Data, 0, Data.Length);// Write the data to the encryption stream.
                    }
                    AesEncryptionData returnData = new AesEncryptionData();
                    returnData.EncryptedData = msEncrypt.ToArray();
                    returnData.IV = aesAlg.IV;
                    return returnData;
                }
            }

        }

        /// <summary>
        /// Takes Data, and encrypts via preset IV and Key. Used for network transmission.
        /// </summary>
        /// <param name="Data">Data to encrypt.</param>
        /// <param name="Key">Key to encrypt with.</param>
        /// <param name="IV">Iv to encrypt with.</param>
        /// <returns>Encrypted data.</returns>
        public static byte[] EncryptBytes_Aes(byte[] Data, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (Data == null || Data.Length <= 0)
                throw new ArgumentNullException("plainText");//No Data!
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");//No Key!
            // Create an Aes object with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Padding = PaddingMode.PKCS7;//Padding mode must match both ends.
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                // Create a encrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);// use provided aes settings to create and encryptor.

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {

                        csEncrypt.Write(Data, 0, Data.Length);// write data through the cypher.
                    }
                    return msEncrypt.ToArray();// read data on far side and return.
                }
            }

        }
        public static byte[] DecryptBytes_Aes(byte[] cipherData, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherData == null || cipherData.Length <= 0)
                throw new ArgumentNullException("cipherData");// No data!
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");//No Key!
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");//No IV!
            // Create an Aes object with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Padding = PaddingMode.PKCS7;// Padding mode must match.
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);// create a decryptor with normal settings.


                try// If we can decrypt
                {
                    using (MemoryStream msDecrypt = new MemoryStream())// create a memory stream
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))// link a crypto stream to it and the decryptor 
                        {
                            csDecrypt.Write(cipherData, 0, cipherData.Length);//write data through it
                        }
                        return msDecrypt.ToArray();// and return the unencrypted data.
                    }
                }
                catch// otherwise return null
                {
                    return null;
                }

            }

        }

        public static byte[] GenerateKey(int keySize)
        {
            AesManaged a = new AesManaged();
            a.KeySize = keySize;
            a.GenerateKey();
            
            return a.Key;
        }
    }
}

