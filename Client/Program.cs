using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ServiceContracts;

namespace Client
{
	public class Program
	{
		static void Main(string[] args)
		{
			NetTcpBinding binding = new NetTcpBinding();
			string address = "net.tcp://localhost:9999/SecurityService";

            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;

            Console.WriteLine("User that started the client is : " + WindowsIdentity.GetCurrent().Name);

            using (ClientProxy proxy = new ClientProxy(binding, address))
            {
                bool running = true;
                while (running)
                {
                    Console.Clear();

                    Console.WriteLine("\n=== Parking Zones ===");
                    var zones = proxy.GetParkingZones();
                    foreach (var zone in zones)
                    {
                        Console.WriteLine(zone);
                    }

                    Console.WriteLine("=== Parking Management System ===");
                    Console.WriteLine("1. Add new parking zone");
                    Console.WriteLine("2. Update existing parking zone");
                    Console.WriteLine("3. Pay for parking");
                    Console.WriteLine("4. Check if parking is paid");
                    Console.WriteLine("5. Issue penalty ticket");
                    Console.WriteLine("6. Delete penalty ticket");
                    Console.WriteLine("7. Exit");
                    Console.Write("Enter your choice: ");

                    if (!int.TryParse(Console.ReadLine(), out int choice))
                    {
                        Console.WriteLine("Invalid input. Please enter a number.");
                        Console.ReadKey();
                        continue;
                    }

                    try
                    {
                        switch (choice)
                        {
                            case 1:
                                AddParkingZone(proxy);
                                break;
                            case 2:
                                UpdateParkingZone(proxy);
                                break;
                            case 3:
                                PayParking(proxy);
                                break;
                            case 4:
                                CheckParkingPayment(proxy);
                                break;
                            case 5:
                                IssuePenaltyTicket(proxy);
                                break;
                            case 6:
                                DeletePenaltyTicket(proxy);
                                break;
                            case 7:
                                running = false;
                                Console.WriteLine("Exiting application...");
                                break;
                            default:
                                Console.WriteLine("Invalid choice. Please try again.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }

                    if (choice != 7)
                    {
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                    }
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(true);

        }


        private static void AddParkingZone(ClientProxy proxy)
        {
            Console.WriteLine("\n=== Add New Parking Zone ===");
            var zone = new ParkingZone();

            Console.Write("Enter Zone ID: ");
            zone.Id = int.Parse(Console.ReadLine());

            Console.Write("Enter Zone Name: ");
            zone.Name = Console.ReadLine();

            Console.Write("Enter Description: ");
            zone.Description = Console.ReadLine();

            Console.Write("Enter Price Per Hour: ");
            zone.PricePerHour = decimal.Parse(Console.ReadLine());

            Console.Write("Enter Active From (HH:mm): ");
            var fromParts = Console.ReadLine().Split(':');
            zone.ActiveFrom = new TimeSpan(int.Parse(fromParts[0]), int.Parse(fromParts[1]), 0);

            Console.Write("Enter Active To (HH:mm): ");
            var toParts = Console.ReadLine().Split(':');
            zone.ActiveTo = new TimeSpan(int.Parse(toParts[0]), int.Parse(toParts[1]), 0);

            zone.IsActive = true;

            bool result = proxy.AddParkingZone(zone);
            Console.WriteLine(result ? "Zone added successfully!" : "Failed to add zone.");
        }

        private static void UpdateParkingZone(ClientProxy proxy)
        {
            Console.WriteLine("\n=== Update Parking Zone ===");
            var zone = new ParkingZone();

            Console.Write("Enter Zone ID to update: ");
            zone.Id = int.Parse(Console.ReadLine());

            Console.Write("Enter New Zone Name: ");
            zone.Name = Console.ReadLine();

            Console.Write("Enter New Description: ");
            zone.Description = Console.ReadLine();

            Console.Write("Enter New Price Per Hour: ");
            zone.PricePerHour = decimal.Parse(Console.ReadLine());

            Console.Write("Enter New Active From (HH:mm): ");
            var fromParts = Console.ReadLine().Split(':');
            zone.ActiveFrom = new TimeSpan(int.Parse(fromParts[0]), int.Parse(fromParts[1]), 0);

            Console.Write("Enter New Active To (HH:mm): ");
            var toParts = Console.ReadLine().Split(':');
            zone.ActiveTo = new TimeSpan(int.Parse(toParts[0]), int.Parse(toParts[1]), 0);

            zone.IsActive = true;

            bool result = proxy.UpdateParkingZone(zone);
            Console.WriteLine(result ? "Zone updated successfully!" : "Failed to update zone.");
        }

        private static void PayParking(ClientProxy proxy)
        {
            Console.WriteLine("\n=== Pay for Parking ===");
            Console.Write("Enter License Plate: ");
            string licensePlate = Console.ReadLine();

            Console.Write("Enter Zone ID: ");
            int zoneId = int.Parse(Console.ReadLine());

            Console.Write("Enter Hours to Pay: ");
            int hours = int.Parse(Console.ReadLine());

            bool result = proxy.PayParking(licensePlate, zoneId, hours);
            Console.WriteLine(result ? "Payment successful!" : "Payment failed.");
        }

        private static void CheckParkingPayment(ClientProxy proxy)
        {
            Console.WriteLine("\n=== Check Parking Payment ===");
            Console.Write("Enter License Plate: ");
            string licensePlate = Console.ReadLine();

            Console.Write("Enter Zone ID: ");
            int zoneId = int.Parse(Console.ReadLine());

            bool isPaid = proxy.IsParkingPaid(licensePlate, zoneId);
            Console.WriteLine(isPaid ? "Parking is paid." : "No valid parking payment found.");
        }

        private static void IssuePenaltyTicket(ClientProxy proxy)
        {
            Console.WriteLine("\n=== Issue Penalty Ticket ===");
            Console.Write("Enter License Plate: ");
            string licensePlate = Console.ReadLine();

            Console.Write("Enter Zone ID: ");
            int zoneId = int.Parse(Console.ReadLine());

            try
            {
                var ticket = proxy.IssuePenaltyTicket(licensePlate, zoneId);
                Console.WriteLine($"Ticket issued successfully!");
                Console.WriteLine($"License: {ticket.LicensePlate}");
                Console.WriteLine($"Zone: {ticket.ZoneId}");
                Console.WriteLine($"Fine: {ticket.FineAmount:C}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error issuing ticket: {ex.Message}");
            }
        }

        private static void DeletePenaltyTicket(ClientProxy proxy)
        {
            Console.WriteLine("\n=== Delete Penalty Ticket ===");
            Console.Write("Enter License Plate: ");
            string licensePlate = Console.ReadLine();

            bool result = proxy.DeletePenaltyTicket(licensePlate);
            Console.WriteLine(result ? "Ticket(s) deleted successfully!" : "Failed to delete ticket(s).");
        }

    }
}
