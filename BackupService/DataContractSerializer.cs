using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts;

namespace BackupService
{
    public class ReplicationDataContainer
    {
        public Dictionary<int, ParkingZone> Zones { get; set; }
        public List<ParkingPayment> Payments { get; set; }
        public List<PenaltyTicket> Tickets { get; set; }
        public List<User> Users { get; set; }
    }
}
