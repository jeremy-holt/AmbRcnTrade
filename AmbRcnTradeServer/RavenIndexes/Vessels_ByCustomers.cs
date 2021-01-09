using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.VesselModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Vessels_ByCustomers : AbstractIndexCreationTask<Vessel, VesselListItem>
    {
        public Vessels_ByCustomers()
        {
            Map = vessels => from vessel in vessels
                let shippingCompany = LoadDocument<Customer>(vessel.ShippingCompanyId)
                let forwardingAgent = LoadDocument<Customer>(vessel.ForwardingAgentId)
                let containersOnBoard = LoadDocument<BillLading>(vessel.BillLadingIds).Sum(c=> c.ContainerIds.Count)
                select new VesselListItem
                {
                    Id = vessel.Id,
                    ContainersOnBoard = containersOnBoard,
                    ForwardingAgentName = forwardingAgent.Name,
                    ShippingCompanyName = shippingCompany.Name,
                    Eta = vessel.Eta,
                    CompanyId = vessel.CompanyId,
                    VesselName = vessel.VesselName
                };

            Index(x => x.Eta, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}