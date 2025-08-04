using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    public class ReplicationData
    {
        [DataMember]
        public byte[] EncryptedZones { get; set; }

        [DataMember]
        public byte[] EncryptedPayments { get; set; }

        [DataMember]
        public byte[] EncryptedTickets { get; set; }

        [DataMember]
        public byte[] DigitalSignature { get; set; }

        [DataMember]
        public byte[] IV { get; set; } // Initialization Vector for CBC mode
    }
}
