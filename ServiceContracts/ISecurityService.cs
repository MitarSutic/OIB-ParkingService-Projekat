using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts
{
	[ServiceContract]
	public interface ISecurityService
	{
		//[OperationContract]
		//void AddUser(string username, string password);

		[OperationContract]
		bool AddParkingZone(ParkingZone parkingZone);

		[OperationContract]
		bool UpdateParkingZone(ParkingZone updatedZone);

		[OperationContract]
	    List<ParkingZone> GetParkingZones();

		[OperationContract]
		bool PayParking(string licensePlate, int zoneID, int hours);

        [OperationContract]
        bool IsParkingPaid(string licensePlate, int zoneId);

        [OperationContract]
        PenaltyTicket IssuePenaltyTicket(string licensePlate, int zoneId);

		[OperationContract]
		bool DeletePenaltyTicket(string licencePlate);

		

    }
}
