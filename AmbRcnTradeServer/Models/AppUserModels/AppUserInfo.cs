using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.AppUserModels
{
    public class AppUserInfo
    {
        public List<string> UserCustomerIds { get; init; } = new();
        public string AppUserId { get; init; }
        public string AppUserName { get; init; }
        public string AppUserRole { get; init; }
    }
}