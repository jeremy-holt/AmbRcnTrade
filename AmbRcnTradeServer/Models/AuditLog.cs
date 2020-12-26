using System;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models
{
    public class AuditLog:IIdentity
    {
        public DateTime Date { get; set; }
        public string UserRole { get; set; }
        public string AppUserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Page { get; set; }
        public string IpAddress { get; set; }
        public string Id { get; set; }
        public string QueryString { get; set; }
    }
}