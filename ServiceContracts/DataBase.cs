using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    
    public class DataBase
    {
        public static Dictionary<int, ParkingZone> zones = new Dictionary<int, ParkingZone>();
        public static List<ParkingPayment> payments = new List<ParkingPayment>();
        public static List<PenaltyTicket> penaltyTickets = new List<PenaltyTicket>();

        static DataBase()
        {
            #region Parking Zones
            zones.Add(1, new ParkingZone
            {
                Id = 1,
                Name = "Green Zone",
                Description = "Low-cost long-term parking.",
                PricePerHour = 0.50m,
                ActiveFrom = new TimeSpan(7, 0, 0),
                ActiveTo = new TimeSpan(20, 0, 0),
                IsActive = true
            });

            zones.Add(2, new ParkingZone
            {
                Id = 2,
                Name = "Red Zone",
                Description = "Premium parking, close to city center.",
                PricePerHour = 2.00m,
                ActiveFrom = new TimeSpan(8, 0, 0),
                ActiveTo = new TimeSpan(22, 0, 0),
                IsActive = true
            });

            zones.Add(3, new ParkingZone
            {
                Id = 3,
                Name = "Blue Zone",
                Description = "Short stay zone, max 1 hour.",
                PricePerHour = 1.00m,
                ActiveFrom = new TimeSpan(6, 0, 0),
                ActiveTo = new TimeSpan(18, 0, 0),
                IsActive = true
            });
            #endregion

        }
    }
}
