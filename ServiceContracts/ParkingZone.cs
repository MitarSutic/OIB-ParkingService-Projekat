using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
    [DataContract]
    public class ParkingZone
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public decimal PricePerHour { get; set; }

        [DataMember]
        public TimeSpan ActiveFrom { get; set; }

        [DataMember]
        public TimeSpan ActiveTo { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        public ParkingZone(int id, string name, string description, decimal pricePerHour, TimeSpan activeFrom, TimeSpan activeTo, bool isActive)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.PricePerHour = pricePerHour;
            this.ActiveFrom = activeFrom;
            this.ActiveTo = activeTo;
            this.IsActive = isActive;
        }

        public ParkingZone() { }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Parking Zone: {Name} (ID: {Id})");
            sb.AppendLine($"Description: {Description}");
            sb.AppendLine($"Price: {PricePerHour:C}/hour");
            sb.AppendLine($"Active Hours: {ActiveFrom:hh\\:mm} - {ActiveTo:hh\\:mm}");
            sb.AppendLine($"Status: {(IsActive ? "Active" : "Inactive")}");

            return sb.ToString();
        }

    }
}
