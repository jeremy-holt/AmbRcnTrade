using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.DictionaryModels
{
    public class Port : IEntityCompany
    {
        public string Country { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
    }
}