using System.Linq;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.VesselModels;
using Raven.Client.Documents.Indexes;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Vessels_ByCustomers:AbstractIndexCreationTask<Vessel, VesselListItem>
    {
        public Vessels_ByCustomers()
        {
            Map = vessels => from c in vessels
                let shippingCompany = LoadDocument<Customer>(c.ShippingCompanyId)
                let forwardingAgent = LoadDocument<Customer>(c.ForwardingAgentId)
                let port = LoadDocument<Port>(c.PortOfDestinationId)
                let etaHistory = c.EtaHistory.FirstOrDefault(x => x.DateUpdated == c.EtaHistory.Max(v => v.DateUpdated))
                select new VesselListItem()
                {
                    Id = c.Id,
                    ContainersOnBoard = c.ContainersOnBoard,
                    ForwardingAgentName = forwardingAgent.Name,
                    ShippingCompanyName = shippingCompany.Name,
                    PortOfDestinationName = port.Name,
                    Eta = etaHistory.Eta,
                    CompanyId=c.CompanyId,
                    VesselName = etaHistory.VesselName
                };
            
            Index(x=>x.Eta,FieldIndexing.Default);
            
            StoreAllFields(FieldStorage.Yes);
        }
    }
}