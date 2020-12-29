using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models
{
    public class ContractQueryParameters
    {
        public string AppUserId { get; set; }
        public string CompanyId { get; set; }
        public string SellerId { get; set; }
        public string BuyerId { get; set; }
        public ContainerStatus? ContainerStatus { get; set; }
    }
}