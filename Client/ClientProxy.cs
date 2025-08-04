using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts;

namespace Client
{
    public class ClientProxy : ChannelFactory<ISecurityService>, ISecurityService, IDisposable
    {
        ISecurityService factory;

        public ClientProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        private void RecreateChannelIfFaulted()
        {
            if (((ICommunicationObject)factory).State == CommunicationState.Faulted)
            {
                ((ICommunicationObject)factory).Abort();
                factory = this.CreateChannel();
            }
        }

        public bool AddParkingZone(ParkingZone parkingZone)
        {
            bool result = false;
            try
            {
                RecreateChannelIfFaulted();
                result = factory.AddParkingZone(parkingZone);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return false;
            }
            return result;
        }

        public List<ParkingZone> GetParkingZones()
        {
            List<ParkingZone> pZone = new List<ParkingZone>();
            try
            {
                RecreateChannelIfFaulted();
                pZone = factory.GetParkingZones();
                
            }
            catch (FaultException fe)
            {
                Console.WriteLine($"Service fault: {fe.Message}");
            }
            return pZone;
        }

        public bool IsParkingPaid(string licensePlate, int zoneId)
        {
            try
            {
                RecreateChannelIfFaulted();
                return factory.IsParkingPaid(licensePlate, zoneId);
            }
            catch (FaultException fe)
            {
                Console.WriteLine($"Service error: {fe.Message}");
                return false;
            }
        }

        public PenaltyTicket IssuePenaltyTicket(string licensePlate, int zoneId)
        {
            PenaltyTicket ticket = new PenaltyTicket();
            try
            {
                RecreateChannelIfFaulted();
                ticket = factory.IssuePenaltyTicket(licensePlate, zoneId);
                return ticket;
            }
            catch (FaultException fe)
            {
                Console.WriteLine($"Service error: {fe.Message}");
                return ticket;
            }
        }

        public bool PayParking(string licensePlate, int zoneId, int hours)
        {
            try
            {
                RecreateChannelIfFaulted();
                return factory.PayParking(licensePlate, zoneId, hours);
            }
            catch (FaultException fe)
            {
                Console.WriteLine($"Service error: {fe.Message}");
                return false;
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine($"Network error: {ce.Message}");
                ((ICommunicationObject)factory).Abort();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public bool UpdateParkingZone(ParkingZone updatedZone)
        {
            bool result = false;
            try
            {
                RecreateChannelIfFaulted();
                result = factory.UpdateParkingZone(updatedZone);
                return result;
                
            }
            catch (FaultException fe)
            {
                Console.WriteLine($"Service fault: {fe.Message}");
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine($"Communication error: {ce.Message}");
                ((ICommunicationObject)factory).Abort();
            }
            catch (TimeoutException te)
            {
                Console.WriteLine($"Timeout error: {te.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"General error: {e.Message}");
            }
            return result;
        }

        public void CheckAndPenalize(string licensePlate, int zoneId)
        {
            try
            {
                RecreateChannelIfFaulted();
                if (!factory.IsParkingPaid(licensePlate, zoneId))
                {
                    
                    var ticket = factory.IssuePenaltyTicket(licensePlate, zoneId);
                    Console.WriteLine($"ISSUED PENALTY TICKET:");
                    Console.WriteLine($"Vehicle: {ticket.LicensePlate}");
                    Console.WriteLine($"Zone: {ticket.ZoneId}");
                    Console.WriteLine($"Fine: {ticket.FineAmount:C}");
                }
                else
                {
                    Console.WriteLine("Vehicle has valid parking payment");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public bool DeletePenaltyTicket(string licencePlate)
        {
            try
            {
                RecreateChannelIfFaulted();
                bool result = factory.DeletePenaltyTicket(licencePlate);
                if (result)
                {
                    Console.WriteLine($"Successfully deleted penalty ticket(s) for license plate: {licencePlate}");
                }
                else
                {
                    Console.WriteLine($"No penalty ticket found for {licencePlate}");
                }
                return result;
            }
            catch (FaultException fe)
            {
                Console.WriteLine($"Service error: {fe.Message}");
                return false;
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine($"Network error: {ce.Message}");
                ((ICommunicationObject)factory).Abort();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
