using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using Logging;
using SecurityManagers;
using ServiceContracts;


namespace SecurityService
{
    public class SecurityService : ISecurityService
	{
        //public static Dictionary<string, User> UserAccountsDB = new Dictionary<string, User>();

        public SecurityService()
        {
            try
            {
                Audit.LogSystemEvent("SecurityService initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log system event: {ex.Message}");
            }
        }

        public bool AddParkingZone(ParkingZone parkingZone)
        {
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;

            if (windowsIdentity == null || !windowsIdentity.IsAuthenticated)
            {
                try
                {
                    Audit.AuthorizationFailed("Unknown", "AddParkingZone", "Not authenticated");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to log authorization failure: {ex.Message}");
                }
                return false;
                //Console.WriteLine("Error: Windows authentication required.");
                //return false;
            }

            try
            {
                Audit.AuthenticationSuccess(windowsIdentity.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log authentication: {ex.Message}");
            }

            Console.WriteLine($"Client attempting to add zone: {windowsIdentity.Name}");

            // Check authorization - user must be in ManageZone group
            bool isAuthorized = windowsIdentity.Groups
                .Select(group => (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier)))
                .Any(sid => {
                    string groupName = sid.Translate(typeof(NTAccount)).ToString();
                    return groupName.EndsWith("\\ManageZone", StringComparison.OrdinalIgnoreCase) ||
                           groupName.Equals("ManageZone", StringComparison.OrdinalIgnoreCase);
                });

            if (!isAuthorized)
            {
                try
                {
                    Audit.AuthorizationFailed(windowsIdentity.Name, "AddParkingZone", "Missing ManageZone privileges");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to log authorization failure: {ex.Message}");
                }
                return false;

                //Console.WriteLine($"Access denied. User '{windowsIdentity.Name}' lacks ManageZone privileges.");
                //return false;
            }

            try
            {
                Audit.AuthorizationSuccess(windowsIdentity.Name, "AddParkingZone");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log authorization success: {ex.Message}");
            }

            try
            {
                if (DataBase.zones.ContainsKey(parkingZone.Id))
                {
                    Audit.DatabaseOperationFailed("AddParkingZone", $"Zone ID {parkingZone.Id} already exists");
                    return false;
                }

                DataBase.zones.Add(parkingZone.Id, parkingZone);
                Audit.DatabaseOperationSuccess("AddParkingZone",
                    $"Added zone ID:{parkingZone.Id}, Name:{parkingZone.Name}");
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    Audit.DatabaseOperationFailed("AddParkingZone", ex.Message);
                }
                catch (Exception logEx)
                {
                    Console.WriteLine($"Failed to log database error: {logEx.Message}");
                }
                return false;
            }


        }

        public List<ParkingZone> GetParkingZones()
        {
            try
            {
                var zones = DataBase.zones.Values.ToList();
                Audit.DatabaseOperationSuccess("GetParkingZones", $"Retrieved {zones.Count} zones");
                return zones;
            }
            catch (Exception ex)
            {
                Audit.DatabaseOperationFailed("GetParkingZones", ex.Message);
                throw;
            }
        }

        public bool IsParkingPaid(string licensePlate, int zoneId)
        {
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;

            if (windowsIdentity == null || !windowsIdentity.IsAuthenticated)
            {
                Audit.AuthorizationFailed("Unknown", "IsParkingPaid", "Not authenticated");
                return false;
            }

            Audit.AuthenticationSuccess(windowsIdentity.Name);

            bool isAuthorized = windowsIdentity.Groups
                .Select(group => (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier)))
                .Any(sid => {
                    string groupName = sid.Translate(typeof(NTAccount)).ToString();
                    return groupName.EndsWith("\\ParkingWorker", StringComparison.OrdinalIgnoreCase) ||
                           groupName.Equals("ParkingWorker", StringComparison.OrdinalIgnoreCase);
                });

            if (!isAuthorized)
            {
                Audit.AuthorizationFailed(windowsIdentity.Name, "IsParkingPaid", "Missing ParkingWorker privileges");
                return false;
            }

            Audit.AuthorizationSuccess(windowsIdentity.Name, "IsParkingPaid");

            try
            {
                licensePlate = licensePlate.Trim().ToUpper();
                var activePayments = DataBase.payments
                    .Where(p => p.LicensePlate == licensePlate &&
                               p.ZoneId == zoneId &&
                               DateTime.Now <= p.PaymentTime.Add(p.Duration))
                    .OrderByDescending(p => p.PaymentTime);

                bool isPaid = activePayments.Any();
                Audit.DatabaseOperationSuccess("IsParkingPaid",
                    $"Checked payment for {licensePlate} in zone {zoneId}. Paid: {isPaid}");
                return isPaid;
            }
            catch (Exception ex)
            {
                Audit.DatabaseOperationFailed("IsParkingPaid", ex.Message);
                return false;
            }
        }

        public PenaltyTicket IssuePenaltyTicket(string licensePlate, int zoneId)
        {
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;

            if (windowsIdentity == null || !windowsIdentity.IsAuthenticated)
            {
                Audit.AuthorizationFailed("Unknown", "IssuePenaltyTicket", "Not authenticated");
                throw new System.Security.SecurityException("Authentication required.");
            }

            Audit.AuthenticationSuccess(windowsIdentity.Name);

            bool isAuthorized = windowsIdentity.Groups
                .Select(group => (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier)))
                .Any(sid => {
                    string groupName = sid.Translate(typeof(NTAccount)).ToString();
                    return groupName.EndsWith("\\ParkingWorker", StringComparison.OrdinalIgnoreCase) ||
                           groupName.Equals("ParkingWorker", StringComparison.OrdinalIgnoreCase);
                });

            if (!isAuthorized)
            {
                Audit.AuthorizationFailed(windowsIdentity.Name, "IssuePenaltyTicket", "Missing ParkingWorker privileges");
                throw new System.Security.SecurityException("Authentication required.");
            }

            Audit.AuthorizationSuccess(windowsIdentity.Name, "IssuePenaltyTicket");

            try
            {
                if (IsParkingPaid(licensePlate, zoneId))
                {
                    throw new InvalidOperationException("Parking is already paid for this vehicle");
                }

                if (!DataBase.zones.TryGetValue(zoneId, out var zone))
                {
                    throw new ArgumentException("Invalid zone ID");
                }

                decimal fineAmount = zone.PricePerHour * 2;
                var ticket = new PenaltyTicket(licensePlate, zoneId, fineAmount);
                DataBase.penaltyTickets.Add(ticket);

                Audit.DatabaseOperationSuccess("IssuePenaltyTicket",
                    $"Issued ticket for {licensePlate} in zone {zoneId}. Fine: {fineAmount:C}");
                return ticket;
            }
            catch (Exception ex)
            {
                Audit.DatabaseOperationFailed("IssuePenaltyTicket", ex.Message);
                throw;
            }
        }

        public bool PayParking(string licensePlate, int zoneID, int hours)
        {
            try
            {
                if (!DataBase.zones.TryGetValue(zoneID, out ParkingZone zone))
                {
                    Audit.DatabaseOperationFailed("PayParking", $"Zone ID {zoneID} not found");
                    return false;
                }

                TimeSpan currentTime = DateTime.Now.TimeOfDay;
                if (!zone.IsActive || currentTime < zone.ActiveFrom || currentTime > zone.ActiveTo)
                {
                    Audit.DatabaseOperationFailed("PayParking",
                        $"Zone '{zone.Name}' is not currently active");
                    return false;
                }

                decimal amount = zone.PricePerHour * hours;
                TimeSpan duration = TimeSpan.FromHours(hours);

                var payment = new ParkingPayment(
                    licensePlate: licensePlate.Trim().ToUpper(),
                    zoneId: zoneID,
                    paymentTime: DateTime.Now,
                    duration: duration,
                    amountPaid: amount
                );

                DataBase.payments.Add(payment);
                Audit.DatabaseOperationSuccess("PayParking",
                    $"Payment recorded: {licensePlate} in {zone.Name} for {hours} hours (Amount: {amount:C})");
                return true;
            }
            catch (Exception ex)
            {
                Audit.DatabaseOperationFailed("PayParking", ex.Message);
                return false;
            }
        }


        public bool UpdateParkingZone(ParkingZone updatedZone)
        {
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;

            if (windowsIdentity == null || !windowsIdentity.IsAuthenticated)
            {
                Audit.AuthorizationFailed("Unknown", "UpdateParkingZone", "Not authenticated");
                return false;
            }

            Audit.AuthenticationSuccess(windowsIdentity.Name);

            bool isAuthorized = windowsIdentity.Groups
                .Select(group => (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier)))
                .Any(sid => {
                    string groupName = sid.Translate(typeof(NTAccount)).ToString();
                    return groupName.EndsWith("\\ManageZone", StringComparison.OrdinalIgnoreCase) ||
                           groupName.Equals("ManageZone", StringComparison.OrdinalIgnoreCase);
                });

            if (!isAuthorized)
            {
                Audit.AuthorizationFailed(windowsIdentity.Name, "UpdateParkingZone", "Missing ManageZone privileges");
                return false;
            }

            Audit.AuthorizationSuccess(windowsIdentity.Name, "UpdateParkingZone");

            try
            {
                if (updatedZone == null)
                {
                    Audit.DatabaseOperationFailed("UpdateParkingZone", "Null parking zone provided");
                    return false;
                }

                if (!DataBase.zones.ContainsKey(updatedZone.Id))
                {
                    Audit.DatabaseOperationFailed("UpdateParkingZone", $"Zone ID {updatedZone.Id} doesn't exist");
                    return false;
                }

                if (updatedZone.ActiveFrom >= updatedZone.ActiveTo)
                {
                    Audit.DatabaseOperationFailed("UpdateParkingZone", "ActiveFrom must be before ActiveTo");
                    return false;
                }

                DataBase.zones[updatedZone.Id] = updatedZone;
                Audit.DatabaseOperationSuccess("UpdateParkingZone",
                    $"Updated zone: {updatedZone.Name} (ID: {updatedZone.Id})");
                return true;
            }
            catch (Exception ex)
            {
                Audit.DatabaseOperationFailed("UpdateParkingZone", ex.Message);
                return false;
            }
        }

        public bool DeletePenaltyTicket(string licensePlate)
        {
            WindowsIdentity windowsIdentity = Thread.CurrentPrincipal.Identity as WindowsIdentity;

            if (windowsIdentity == null || !windowsIdentity.IsAuthenticated)
            {
                Audit.AuthorizationFailed("Unknown", "DeletePenaltyTicket", "Not authenticated");
                return false;
            }

            Audit.AuthenticationSuccess(windowsIdentity.Name);

            bool isAuthorized = windowsIdentity.Groups
                .Select(group => (SecurityIdentifier)group.Translate(typeof(SecurityIdentifier)))
                .Any(sid => {
                    string groupName = sid.Translate(typeof(NTAccount)).ToString();
                    return groupName.EndsWith("\\ParkingWorker", StringComparison.OrdinalIgnoreCase) ||
                           groupName.Equals("ParkingWorker", StringComparison.OrdinalIgnoreCase);
                });

            if (!isAuthorized)
            {
                Audit.AuthorizationFailed(windowsIdentity.Name, "DeletePenaltyTicket", "Missing ParkingWorker privileges");
                return false;
            }

            Audit.AuthorizationSuccess(windowsIdentity.Name, "DeletePenaltyTicket");

            try
            {
                licensePlate = licensePlate.Trim().ToUpper();
                var ticketsToDelete = DataBase.penaltyTickets
                    .Where(t => t.LicensePlate.Equals(licensePlate, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!ticketsToDelete.Any())
                {
                    Audit.DatabaseOperationFailed("DeletePenaltyTicket", $"No tickets found for {licensePlate}");
                    return false;
                }

                foreach (var ticket in ticketsToDelete)
                {
                    DataBase.penaltyTickets.Remove(ticket);
                }

                Audit.DatabaseOperationSuccess("DeletePenaltyTicket",
                    $"Deleted {ticketsToDelete.Count} ticket(s) for {licensePlate}");
                return true;
            }
            catch (Exception ex)
            {
                Audit.DatabaseOperationFailed("DeletePenaltyTicket", ex.Message);
                return false;
            }
        }

        public byte[] GetReplicationData()
        {
            throw new NotImplementedException();
        }

        public void TestCommunication()
        {
            Console.WriteLine("Communication established.");
        }
    }
}


