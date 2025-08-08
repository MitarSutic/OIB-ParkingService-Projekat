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
        public Dictionary<int,ParkingZone> Zones { get; set; }

        [DataMember]
        public List<ParkingPayment> Payments { get; set; }

        [DataMember]
        public List<PenaltyTicket> Tickets { get; set; }    
    }
}
