using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.VesselModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Containers_ByBillLading : AbstractMultiMapIndexCreationTask<Containers_ByBillLading.Result>
    {
        public class Result
        {
            public string VesselId { get; set; }
            public ContainerStatus Status { get; set; }
            public string CompanyId { get; set; }
            public string ContainerId { get; set; }
        }

        public Containers_ByBillLading()
        {
            AddMap<Container>(containers => from container in containers
                let doc = LoadDocument<Container>(container.Id)
                select new 
                {
                    ContainerId = container.Id,
                    container.Status,
                    VesselId = default(string),
                    doc.Bags,
                    doc.BookingNumber,
                    doc.CompanyId,
                    doc.ContainerNumber,
                    container.DispatchDate,
                    container.SealNumber,
                    container.StuffingWeightKg,
                    container.VgmTicketNumber,
                    container.StuffingDate,
                    container.NettWeightKg,
                    container.WeighbridgeWeightKg
                }
            );

            AddMap<Vessel>(vessels => from vessel in vessels
                from billLadingId in vessel.BillLadingIds
                let billLading = LoadDocument<BillLading>(billLadingId)
                let containers = LoadDocument<Container>(billLading.ContainerIds)
                from container in containers
                let doc = LoadDocument<Container>(container.Id)
                select new 
                {
                    ContainerId =container.Id,
                    Status = default(object),
                    VesselId = vessel.Id,
                    doc.Bags,
                    doc.BookingNumber,
                    doc.CompanyId,
                    doc.ContainerNumber,
                    doc.DispatchDate,
                    doc.SealNumber,
                    doc.StuffingWeightKg,
                    doc.VgmTicketNumber,
                    doc.StuffingDate,
                    doc.NettWeightKg,
                    doc.WeighbridgeWeightKg
                });

            Index(x => x.VesselId, FieldIndexing.Default);
            Index(x => x.ContainerId, FieldIndexing.Default);
            Index(x => x.Status, FieldIndexing.Default);
            Index(x=>x.CompanyId, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}