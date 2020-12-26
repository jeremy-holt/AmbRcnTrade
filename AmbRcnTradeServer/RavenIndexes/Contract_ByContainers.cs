using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Models;
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
                select new ContractListItem
                {
                    Id = c.Id,
                    CompanyId = c.CompanyId,
                    ContractNumber = c.ContractNumber,
                    ContractDate = c.ContractDate,
                    Seller = new ListItem {Id = c.SellerId, Name = seller.Name},
                    Buyer = new ListItem {Id = c.BuyerId, Name = buyer.Name},
                    Broker = new ListItem {Id = c.BrokerId, Name = broker.Name},
                    ContainerNumber = container.ContainerNumber,
                    VesselName = container.VesselName,
                    VesselEta = container.VesselEta,
                    BlNumber = container.BlNumber,
                    BlDate = container.BlDate,
                    Status = container.Status
                };

            Index(x => x.Seller.Id, FieldIndexing.Default);
            Index(x => x.Buyer.Id, FieldIndexing.Default);
            Index(x => x.Broker.Id, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}