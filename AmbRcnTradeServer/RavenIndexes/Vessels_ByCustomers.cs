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
            Map = vessels => from vessel in vessels
                let shippingCompany = LoadDocument<Customer>(vessel.ShippingCompanyId)
                let forwardingAgent = LoadDocument<Customer>(vessel.ForwardingAgentId)
                let port = LoadDocument<Port>(vessel.PortOfDestinationId)
                select new VesselListItem()
                {
                    Id = vessel.Id,
                    ContainersOnBoard = vessel.ContainersOnBoard,
                    ForwardingAgentName = forwardingAgent.Name,
                    ShippingCompanyName = shippingCompany.Name,
                    PortOfDestinationName = port.Name,
                    Eta = vessel.Eta,
                    CompanyId=vessel.CompanyId,
                    VesselName = vessel.VesselName
                };
            
            Index(x=>x.Eta,FieldIndexing.Default);
            
            StoreAllFields(FieldStorage.Yes);
        }
    }
}