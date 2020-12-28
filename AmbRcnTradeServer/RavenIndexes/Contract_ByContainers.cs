using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.DictionaryModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Contract_ByContainers : AbstractIndexCreationTask<Contract, ContractListItem>
    {
        public Contract_ByContainers()
        {
            Map = contracts => from c in contracts
                let seller = LoadDocument<Customer>(c.SellerId)
                let buyer = LoadDocument<Customer>(c.BuyerId)
                let broker = LoadDocument<Customer>(c.BrokerId)
                from container in c.Containers
                select new
                {
                    c.Id,
                    c.CompanyId,
                    c.ContractNumber,
                    c.ContractDate,
                    c.SellerId,
                    SellerName = seller.Name,
                    c.BuyerId,
                    BuyerName = buyer.Name,
                    c.BrokerId,
                    BrokerName = broker.Name,
                    container.ContainerNumber,
                    container.VesselName,
                    container.VesselEta,
                    container.BlNumber,
                    container.BlDate,
                    container.Status
                };

            Index(x => x.SellerId, FieldIndexing.Default);
            Index(x => x.BuyerId, FieldIndexing.Default);
            Index(x => x.BrokerId, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);
            Index(x => x.Status, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}