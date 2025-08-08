using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Logging;
using Newtonsoft.Json;
using SecurityManagers;
using ServiceContracts;

namespace SecurityService
{
    public class PrimaryService : IPrimaryService
    {
        private const string DesKey = "8ByteKey"; // DES requires exactly 8-byte key
        private const CipherMode EncryptionMode = CipherMode.CBC;

        public byte[] GetReplicationData()
        {
            try
            {
                var dataToReplicate = new
                {
                    Zones = DataBase.zones,
                    Payments = DataBase.payments,
                    Tickets = DataBase.penaltyTickets
                };

                // System.Text.Json explicit usage
                string jsonData = System.Text.Json.JsonSerializer.Serialize(dataToReplicate);
                byte[] plainData = Encoding.UTF8.GetBytes(jsonData);

                byte[] encryptedData = DES_Symn_Algorithm.EncryptData(
                    plainData,
                    DesKey,
                    EncryptionMode);

                return encryptedData;
            }
            catch (Exception ex)
            {
                Audit.LogSystemEvent($"Replication failed: {ex.Message}");
                throw;
            }
        }

        public void TestCommunication()
        {
            Console.WriteLine("Comm test");
        }

        private string GetDataToReplicate()
        {
            // Implement your actual data collection logic here
            // Example: Get all parking transactions since last replication
            var transactions = DataBase.zones;
            return JsonConvert.SerializeObject(transactions);
        }
    }
}
