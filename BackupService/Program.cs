using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Text.Json;
using System.Threading;
using BackupService;
using SecurityManager;
using SecurityManagers;
using ServiceContracts;

public class Program
{
    private const string PrimaryServiceUrl = "net.tcp://localhost:9998/BackupService";
    private const string ReplicaFilePath = "replicatedData.dat";
    private const string DesKey = "8ByteKey";
    private static readonly TimeSpan ReplicationInterval = TimeSpan.FromSeconds(60);
    private const string BackupCertName = "backupservice"; // CN backup sertifikata
    private const string PrimaryCertName = "mainservice"; // CN primarnog sertifikata
    //private const string BackupCertThumbprint = "1e40bb7731b8a5d6c848ab33e3a69e397c639dd3";

    static void Main(string[] args)
    {
        NetTcpBinding binding = new NetTcpBinding();
        binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
        binding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;

        X509Certificate2 srvCert = DigitalSignatureManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, PrimaryCertName);
        EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:9998/BackupService"),
                                  new X509CertificateEndpointIdentity(srvCert));

        // Create a timer that will trigger every minute
        Timer timer = new Timer(async state =>
        {
            try
            {
                using (WCFBackupService proxy = new WCFBackupService(binding, address))
                {
                    Console.WriteLine($"[{DateTime.Now}] Requesting replication data...");

                    // 1. First test communication
                    proxy.TestCommunication();
                    Console.WriteLine("Communication test successful");

                    // 2. Get replication data
                    byte[] replicationData = proxy.GetReplicationData();
                    byte[] decrypted = DES_Symn_Algorithm.DecryptData(replicationData, DesKey, CipherMode.CBC);
                    string jsonData = Encoding.UTF8.GetString(decrypted);
                    Console.WriteLine("Decrypted JSON Data:");
                    Console.WriteLine(jsonData);

                    // 3. Deserialize to view structured data
                    var data = JsonSerializer.Deserialize<ReplicationData>(jsonData);
                    // 4. Display formatted output
                    Console.WriteLine("\nFormatted Data:");
                    Console.WriteLine("\n=== Zones ===");
                    foreach (var zone in data.Zones.Values)
                    {
                        Console.WriteLine($"ID: {zone.Id}");
                        Console.WriteLine($"Name: {zone.Name}");
                        Console.WriteLine($"Description: {zone.Description}");
                        Console.WriteLine($"Price: {zone.PricePerHour:C}/hour");
                        Console.WriteLine($"Hours: {zone.ActiveFrom} to {zone.ActiveTo}");
                        Console.WriteLine($"Status: {(zone.IsActive ? "Active" : "Inactive")}");
                        Console.WriteLine("------------------");
                    }

                    Console.WriteLine("\n=== Payments ===");
                    Console.WriteLine($"Count: {data.Payments.Count}");

                    Console.WriteLine("\n=== Tickets ===");
                    Console.WriteLine($"Count: {data.Tickets.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Error during replication: {ex.Message}");
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1)); // Start immediately, repeat every minute

        Console.WriteLine("Backup service started. Replication will occur every minute. Press <enter> to exit...");
        Console.ReadLine();

        // Clean up the timer when exiting
        timer.Dispose();
    }


    static void ValidateCertificates()
    {
        using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
        {
            store.Open(OpenFlags.ReadOnly);

            // Provera backup sertifikata
            var backupCert = store.Certificates
                .Find(X509FindType.FindBySubjectName, BackupCertName, false)
                .OfType<X509Certificate2>()
                .FirstOrDefault();

            if (backupCert == null)
                throw new Exception($"Backup certificate {BackupCertName} not found");

            Console.WriteLine($"Using backup certificate: {backupCert.Subject}");
        }
    }

    //static void RequestData(object state)
    //{
    //    try
    //    {
    //        Console.WriteLine($"Starting replication at {DateTime.Now}");

    //        byte[] encryptedData = GetDataFromPrimary();
    //        byte[] decryptedData = ProcessReceivedData(encryptedData);
    //        SaveData(decryptedData);

    //        Console.WriteLine($"Replication completed. Data size: {decryptedData.Length} bytes");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Replication error: {ex.Message}");
    //    }
    //}

    //static byte[] GetDataFromPrimary()
    //{
    //    var binding = new NetTcpBinding(SecurityMode.Transport)
    //    {
    //        Security =
    //    {
    //        Transport =
    //        {
    //            ClientCredentialType = TcpClientCredentialType.Certificate,
    //            ProtectionLevel = ProtectionLevel.EncryptAndSign
    //        }
    //    }
    //    };

    //    var endpoint = new EndpointAddress(PrimaryServiceUrl);
    //    var factory = new ChannelFactory<IPrimaryService>(binding, endpoint);

    //    // Koristite FindByThumbprint umesto FindBySubjectName
    //    factory.Credentials.ClientCertificate.SetCertificate(
    //        StoreLocation.LocalMachine,
    //        StoreName.My,
    //        X509FindType.FindByThumbprint,
    //        BackupCertThumbprint);

    //    try
    //    {
    //        Console.WriteLine("Connecting to primary server...");
    //        var proxy = factory.CreateChannel();
    //        byte[] data = proxy.PrepareReplicationData();
    //        ((IClientChannel)proxy).Close();
    //        return data;
    //    }
    //    finally
    //    {
    //        factory.Close();
    //    }
    //}


    static bool VerifySignatureWithCert(byte[] data, byte[] signature, string certName)
    {
        using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
        {
            store.Open(OpenFlags.ReadOnly);
            var cert = store.Certificates
                .Find(X509FindType.FindBySubjectName, certName, false)
                .OfType<X509Certificate2>()
                .FirstOrDefault();

            if (cert == null)
                throw new Exception($"Certificate {certName} not found");

            using (var rsa = cert.GetRSAPublicKey())
            {
                if (rsa == null)
                    throw new Exception("Certificate doesn't have public key");

                using (var sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(data);
                    return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
        }
    }

    static void SaveData(byte[] data)
    {
        File.WriteAllBytes(ReplicaFilePath, data);
        Console.WriteLine($"Data saved to {ReplicaFilePath}");
    }
}