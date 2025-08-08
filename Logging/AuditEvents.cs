using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Resources;

namespace Logging
{
    public enum AuditEventsTypes
    {
        AuthenticationSuccess = 0,
        AuthorizationSuccess = 1,
        AuthorizationFailed = 2,
        DatabaseOperationSuccess = 3,
        DatabaseOperationFailed = 4,
        SystemEvent = 5,
        BackupStarted = 6,
        BackupCompleted = 7,
        BackupFailed = 8,
        DataDecrypted = 9,
        SignatureVerified = 10,
        CertificateValidated = 11,
        Failover = 12
    }

    public class AuditEvents
    {
        private static ResourceManager resourceManager = null;
        private static object resourceLock = new object();

        private static ResourceManager ResourceManager
        {
            get
            {
                lock (resourceLock)
                {
                    if (resourceManager == null)
                    {
                        resourceManager = new ResourceManager
                            (typeof(Resource1).ToString(),
                            Assembly.GetExecutingAssembly());
                    }
                    return resourceManager;
                }
            }
        }

        public static string AuthenticationSuccess
        {
            get
            {
                return ResourceManager.GetString(AuditEventsTypes.AuthenticationSuccess.ToString());
            }
        }

        public static string AuthorizationSuccess
        {
            get
            {               
                return ResourceManager.GetString(AuditEventsTypes.AuthorizationSuccess.ToString());
            }
        }

        public static string AuthorizationFailed
        {
            get
            {             
                return ResourceManager.GetString(AuditEventsTypes.AuthorizationFailed.ToString());
            }
        }

        public static string DatabaseOperationSuccess
        {
            get
            {
                return ResourceManager.GetString(AuditEventsTypes.DatabaseOperationSuccess.ToString());
            }
        }

        public static string DatabaseOperationFailed
        {
            get
            {
                return ResourceManager.GetString(AuditEventsTypes.DatabaseOperationFailed.ToString());
            }
        }

        public static string SystemEvent
        {
            get
            {
                return ResourceManager.GetString(AuditEventsTypes.SystemEvent.ToString());
            }
        }

        public static string BackupStarted
        {
            get { return ResourceManager.GetString(AuditEventsTypes.BackupStarted.ToString()); }
        }

        public static string BackupCompleted
        {
            get { return ResourceManager.GetString(AuditEventsTypes.BackupCompleted.ToString()); }
        }

        public static string BackupFailed
        {
            get { return ResourceManager.GetString(AuditEventsTypes.BackupFailed.ToString()); }
        }

        public static string DataDecrypted
        {
            get { return ResourceManager.GetString(AuditEventsTypes.DataDecrypted.ToString()); }
        }

        public static string SignatureVerified
        {
            get { return ResourceManager.GetString(AuditEventsTypes.SignatureVerified.ToString()); }
        }

        public static string CertificateValidated
        {
            get { return ResourceManager.GetString(AuditEventsTypes.CertificateValidated.ToString()); }
        }

        public static string Failover
        {
            get { return ResourceManager.GetString(AuditEventsTypes.Failover.ToString()); }
        }

    }
}
