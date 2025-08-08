using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Security;
using ServiceContracts;

namespace SecurityService
{
    public class Program
    {
        const string WindowsAuthAddress = "net.tcp://localhost:9999/SecurityService";
        const string CertificateAuthAddress = "net.tcp://localhost:9998/BackupService";
        const string DataFilePath = "C:\\Users\\mitar\\OneDrive\\Desktop\\Projekat\\ParkingService";
        private const string DesKey = "8ByteKey";

        public static void Main(string[] args)
        {
            // Host for Windows-authenticated clients
            var windowsBinding = new NetTcpBinding();
            windowsBinding.Security.Mode = SecurityMode.Transport;
            windowsBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            windowsBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;

            // Host for Certificate-authenticated backup server
            var certBinding = new NetTcpBinding();
            certBinding.Security.Mode = SecurityMode.Transport;
            certBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

            ServiceHost host = new ServiceHost(typeof(SecurityService));
            ServiceHost hostB = new ServiceHost(typeof(PrimaryService));

            // Add both endpoints
            host.AddServiceEndpoint(
                typeof(ISecurityService),
                windowsBinding,
                WindowsAuthAddress);

            hostB.AddServiceEndpoint(
                typeof(IPrimaryService),
                certBinding,
                CertificateAuthAddress);

            // Configure certificate validation
            hostB.Credentials.ClientCertificate.Authentication.CertificateValidationMode =
                X509CertificateValidationMode.ChainTrust;
            hostB.Credentials.ClientCertificate.Authentication.RevocationMode =
                X509RevocationMode.NoCheck;

            // Set the service certificate (WCFService)
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindBySubjectName, "mainservice", false);
            hostB.Credentials.ServiceCertificate.Certificate = certs[0];
            store.Close();

            try
            {
                host.Open();
                hostB.Open();
                Console.WriteLine($"Service started at:\n" +
                    $"- {WindowsAuthAddress} (Windows auth)\n" +
                    $"- {CertificateAuthAddress} (Certificate auth)");
                Console.WriteLine($"Service certificate: {hostB.Credentials.ServiceCertificate.Certificate.Subject}");
                Console.ReadLine();
                host.Close();
                hostB.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Host error: {ex.Message}");
                host.Abort();
            }
        }
    }
}