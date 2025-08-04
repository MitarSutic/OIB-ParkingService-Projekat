using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    [DataContract]
    public class ParkingPayment
    {
        [DataMember]
        public string LicensePlate { get; set; }

        [DataMember]
        public int ZoneId { get; set; }
        [DataMember]
        public DateTime PaymentTime { get; set; }

        [DataMember]
        public TimeSpan Duration { get; set; }

        [DataMember]
        public decimal AmountPaid { get; set; }

        public ParkingPayment(string licensePlate, int zoneId, DateTime paymentTime, TimeSpan duration, decimal amountPaid)
        {
            LicensePlate = licensePlate;
            ZoneId = zoneId;
            PaymentTime = paymentTime;
            Duration = duration;
            AmountPaid = amountPaid;
        }
    }
}
