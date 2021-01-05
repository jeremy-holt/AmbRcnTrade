using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.DictionaryModels
{
    public class CustomerListItem
    {
        public CustomerListItem(string id, string name, string companyId, CustomerGroup filter)
        {
            Id = id;
            Name = name;
            CompanyId = companyId;
            Filter = filter;
        }

        public CustomerListItem() { }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; }
        public CustomerGroup Filter { get; set; }
    }
}