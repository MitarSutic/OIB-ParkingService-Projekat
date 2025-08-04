using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Text;


namespace Logging
{
    public class Audit : IDisposable
    {
        private static EventLog customLog = null;
        const string SourceName = "ParkingSystem.Audit";
        const string LogName = "ParkingSystemLog";

        static Audit()
        {
            try
            {
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, LogName);
                }
                customLog = new EventLog(LogName,
                    Environment.MachineName, SourceName);
            }
            catch (Exception e)
            {
                customLog = null;
                Console.WriteLine("Error while trying to create log handle. Error = {0}", e.Message);
            }
        }

        public static void AuthenticationSuccess(string userName)
        {

            if (customLog != null)
            {
                string UserAuthenticationSuccess =
                    AuditEvents.AuthenticationSuccess;
                string message = String.Format(UserAuthenticationSuccess,
                    userName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventsTypes.AuthenticationSuccess));
            }
        }

        public static void AuthorizationSuccess(string userName, string serviceName)
        {
            //TO DO
            if (customLog != null)
            {
                string AuthorizationSuccess =
                    AuditEvents.AuthorizationSuccess;
                string message = String.Format(AuthorizationSuccess,
                    userName, serviceName);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventsTypes.AuthorizationSuccess));
            }
        }

        public static void AuthorizationFailed(string userName, string serviceName, string reason)
        {
            if (customLog != null)
            {
                string AuthorizationFailed =
                    AuditEvents.AuthorizationFailed;
                string message = String.Format(AuthorizationFailed,
                    userName, serviceName, reason);
                customLog.WriteEntry(message);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventsTypes.AuthorizationFailed));
            }
        }

        public static void DatabaseOperationSuccess(string operation, string details)
        {
            if (customLog != null)
            {
                string message = String.Format(AuditEvents.DatabaseOperationSuccess,
                    operation, details);
                customLog.WriteEntry(message, EventLogEntryType.Information,
                    (int)AuditEventsTypes.DatabaseOperationSuccess);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventsTypes.DatabaseOperationSuccess));
            }
        }

        public static void DatabaseOperationFailed(string operation, string error)
        {
            if (customLog != null)
            {
                string message = String.Format(AuditEvents.DatabaseOperationFailed,
                    operation, error);
                customLog.WriteEntry(message, EventLogEntryType.Error,
                    (int)AuditEventsTypes.DatabaseOperationFailed);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventsTypes.DatabaseOperationFailed));
            }
        }

        public static void LogSystemEvent(string message)
        {
            if (customLog != null)
            {
                customLog.WriteEntry(String.Format(AuditEvents.SystemEvent, message),
                    EventLogEntryType.Information,
                    (int)AuditEventsTypes.SystemEvent);
            }
            else
            {
                throw new ArgumentException(string.Format("Error while trying to write event (eventid = {0}) to event log.",
                    (int)AuditEventsTypes.SystemEvent));
            }
        }

        public void Dispose()
        {
            if (customLog != null)
            {
                customLog.Dispose();
                customLog = null;
            }
        }
    }
}
