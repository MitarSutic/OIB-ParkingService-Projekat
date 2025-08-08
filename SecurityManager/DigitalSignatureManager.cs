using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public static class DigitalSignatureManager
    {
        public static bool VerifySignature(string filePath, string signaturePath, string publicKeyPath)
        {
            byte[] data = File.ReadAllBytes(filePath);                 // original decrypted data
            byte[] signature = File.ReadAllBytes(signaturePath);       // signature to verify
            string publicKeyXml = File.ReadAllText(publicKeyPath);     // public key in XML

            using (SHA256 sha256 = SHA256.Create())
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(publicKeyXml);
                byte[] hash = sha256.ComputeHash(data);

                return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }
    }
}
