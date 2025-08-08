using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class DES_Symn_Algorithm
    {
        public static byte[] EncryptData(byte[] data, string secretKey, CipherMode mode)
        {
            using (DES des = DES.Create())
            {
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
            using (DES des = DES.Create())
            {
                des.Key = Encoding.ASCII.GetBytes(secretKey);
                des.Mode = mode;
                des.Padding = PaddingMode.PKCS7;

                if (mode == CipherMode.CBC)
                {
                    // Extract IV (first 8 bytes)
                    byte[] iv = encryptedDataWithIV.Take(8).ToArray();
                    byte[] ciphertext = encryptedDataWithIV.Skip(8).ToArray();
                    des.IV = iv;

                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(ciphertext, 0, ciphertext.Length);
                        return ms.ToArray();
                    }
                }
                else // ECB
                {
                    using (MemoryStream ms = new MemoryStream())
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedDataWithIV, 0, encryptedDataWithIV.Length);
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
