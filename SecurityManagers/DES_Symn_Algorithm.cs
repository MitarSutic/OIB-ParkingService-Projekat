using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManagers
{
    public class DES_Symn_Algorithm
    {
        public static byte[] EncryptData(byte[] data, string secretKey, CipherMode mode)
        {
            using (DES des = DES.Create())
            {
                
                byte[] keyBytes = Encoding.ASCII.GetBytes(secretKey);
                if (des.Key.Length != 8)
                {
                    throw new ArgumentException("DES key must be exactly 8 ASCII characters (64 bits)");
                }
                des.Key = Encoding.ASCII.GetBytes(secretKey);
                des.Mode = mode;
                des.Padding = PaddingMode.PKCS7; // Better than PaddingMode.None

                if (mode == CipherMode.CBC)
                {
                    des.GenerateIV(); // Auto-generate IV for CBC
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // Write IV first (needed for decryption)
                        ms.Write(des.IV, 0, des.IV.Length);
                        using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                        }
                        return ms.ToArray();
                    }
                }
                else // ECB (not recommended)
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        return ms.ToArray();
                    }
                }
            }
        }


        /// <summary>
        /// Function that decrypts the cipher text from inFile and stores as plaintext to outFile
        /// </summary>
        /// <param name="inFile"> filepath where cipher text is stored </param>
        /// <param name="outFile"> filepath where plain text is expected to be stored </param>
        /// <param name="secretKey"> symmetric encryption key </param>
        public static byte[] DecryptData(byte[] encryptedDataWithIV, string secretKey, CipherMode mode)
        {
            // Validate key length (DES requires exactly 8 bytes)
            if (Encoding.ASCII.GetByteCount(secretKey) != 8)
                throw new ArgumentException("DES key must be exactly 8 ASCII characters");

            using (DES des = DES.Create())
            {
                des.Key = Encoding.ASCII.GetBytes(secretKey);
                des.Mode = mode;
                des.Padding = PaddingMode.PKCS7;

                if (mode == CipherMode.CBC)
                {
                    // Verify minimum length (IV + at least 1 byte of ciphertext)
                    if (encryptedDataWithIV.Length < 9)
                        throw new ArgumentException("Invalid data length for CBC mode");

                    // Extract IV (first 8 bytes)
                    byte[] iv = new byte[8];
                    Buffer.BlockCopy(encryptedDataWithIV, 0, iv, 0, 8);
                    des.IV = iv;

                    // Get ciphertext (remaining bytes after IV)
                    byte[] ciphertext = new byte[encryptedDataWithIV.Length - 8];
                    Buffer.BlockCopy(encryptedDataWithIV, 8, ciphertext, 0, ciphertext.Length);

                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(ciphertext, 0, ciphertext.Length);
                            // MUST call FlushFinalBlock() for proper PKCS7 padding
                            cs.FlushFinalBlock();
                        }
                        return ms.ToArray();
                    }
                }
                else // ECB
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(encryptedDataWithIV, 0, encryptedDataWithIV.Length);
                            // MUST call FlushFinalBlock() for proper PKCS7 padding
                            cs.FlushFinalBlock();
                        }
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
