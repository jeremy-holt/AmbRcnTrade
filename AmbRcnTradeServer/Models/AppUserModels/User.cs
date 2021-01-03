using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.AppUserModels
{
    public class User
    {
        public User(string appUserId, string name)
        {
            AppUserId = appUserId;
            Name = name;
        }

        public User() { }
        public string AppUserId { get; set; }
        public string Name { get; set; }
    }
}