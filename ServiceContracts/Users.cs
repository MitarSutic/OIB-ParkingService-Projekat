using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
	[DataContract]
    public class User
    {
        [DataMember]
        public string WindowsIdentity { get; set; }  // e.g., "DOMAIN\username"

        [DataMember]
        public string DisplayName { get; set; }      // Optional: Friendly name

        [DataMember]
        public List<string> Roles { get; set; }     // e.g., "AdminZone", "ParkingWorker"

        public User(string windowsIdentity, string displayName, List<string> roles)
        {
            WindowsIdentity = windowsIdentity;
            DisplayName = displayName;
            Roles = roles;
        }

        // Default constructor for serialization
        public User() { }
    }
}
