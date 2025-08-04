using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    [DataContract]
    public class PenaltyTicket
    {
        [DataMember]
        public string LicensePlate { get; set; }

        [DataMember]
        public int ZoneId { get; set; }

        [DataMember]
        public decimal FineAmount { get; set; }

        public PenaltyTicket(string licensePlate, int zoneId, decimal fineAmount)
        {
            {
                LicensePlate = licensePlate;
                ZoneId = zoneId;
                FineAmount = fineAmount;
            }
        }

        public PenaltyTicket()
        {
        }
    }
}
