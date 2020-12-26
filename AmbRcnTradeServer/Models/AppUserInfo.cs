using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models
{
    public class AppUserInfo
    {
        public List<string> UserCustomerIds { get; set; } = new List<string>();
        public string AppUserId { get; set; }
        public string AppUserName { get; set; }
        public string AppUserRole { get; set; }
    }
}