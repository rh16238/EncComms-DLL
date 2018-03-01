using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EncCommsPacketsDll
{
    /// <summary>
    /// Handles hashing bytes.
    /// </summary>
    static class PBKDF2Hasher 
    {
        public const int HashByteSize = 32; 
        public const int Pbkdf2Iterations = 1000; //Larger iterations may increase security.
        /// <summary>
        /// Hashes a string according to a salt and const values. Used for passwords.
        /// </summary>
        /// <param name="password">String to hash.</param>
        /// <param name="salt">Salt to hash string with.</param>
        public static byte[] GenerateHash(string password, byte[] salt)
        {
            Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt);//Create new Hasher
            pbkdf2.IterationCount = Pbkdf2Iterations;//Set its iteration count
            return pbkdf2.GetBytes(HashByteSize);//Set it to hash.
        }
    }
}
