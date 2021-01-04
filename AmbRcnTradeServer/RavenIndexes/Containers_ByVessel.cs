using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.VesselModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Containers_ByVessel : AbstractMultiMapIndexCreationTask<NotLoadedContainer>
    {
        public Containers_ByVessel()
        {
            AddMap<Container>(containers => from container in containers
                let doc = LoadDocument<Container>(container.Id)
                select new NotLoadedContainer
                {
                    ContainerId = container.Id,
                    Status = container.Status,
                    VesselId = default,
                    Bags = doc.Bags,
                    BookingNumber = doc.BookingNumber,
                    CompanyId = doc.CompanyId,
                    ContainerNumber = doc.ContainerNumber
                }
            );

            AddMap<Vessel>(vessels => from vessel in vessels
                from containerId in vessel.ContainerIds
                let doc = LoadDocument<Container>(containerId)
                select new NotLoadedContainer
                {
                    ContainerId = containerId,
                    Status = default,
                    VesselId = vessel.Id,
                    Bags = doc.Bags,
                    BookingNumber = doc.BookingNumber,
                    CompanyId = doc.CompanyId,
                    ContainerNumber = doc.ContainerNumber
                });

            Index(x => x.VesselId, FieldIndexing.Default);
            Index(x => x.ContainerId, FieldIndexing.Default);
            Index(x => x.Status, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}