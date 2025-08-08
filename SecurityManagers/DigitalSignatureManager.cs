using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Logging;

namespace SecurityManager
{
    public static class DigitalSignatureManager
    {
        public static X509Certificate2 GetCertificateFromStorage(StoreName storeName, StoreLocation storeLocation, string subjectName)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                // Log all certificates in the store for debugging
                Console.WriteLine($"Available certificates in {storeLocation}\\{storeName}:");
                foreach (var cert in store.Certificates)
                {
                    Console.WriteLine($"- Subject: {cert.Subject}, Thumbprint: {cert.Thumbprint}");
                }

                // Find the certificate
                var certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, false);
                if (certs.Count == 0)
                {
                    throw new Exception($"Certificate with subject '{subjectName}' not found in {storeLocation}\\{storeName}.");
                }

                return certs[0];
            }
        }
    }
}
