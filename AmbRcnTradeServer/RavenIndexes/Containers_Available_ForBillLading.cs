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
    public class Containers_Available_ForBillLading : AbstractMultiMapIndexCreationTask<Containers_Available_ForBillLading.Result>
    {
        public Containers_Available_ForBillLading()
        {
            AddMap<Container>(containers => from container in containers
                select new
                {
                    container.Id,
                    container.Status,
                    container.CompanyId,
                    container.ContainerNumber,
                    container.Bags,
                    container.StuffingWeightKg,
                    container.SealNumber,
                    container.WeighbridgeWeightKg,
                    container.BookingNumber
                }
            );

            AddMap<BillLading>(billsLading => from bl in billsLading
                from containerId in bl.ContainerIds
                let container = LoadDocument<Container>(containerId)
                select new
                {
                    container.Id,
                    container.Status,
                    container.CompanyId,
                    container.ContainerNumber,
                    container.Bags,
                    container.StuffingWeightKg,
                    container.SealNumber,
                    container.WeighbridgeWeightKg,
                    container.BookingNumber
                }
            );

            Reduce = results => from container in results
                group container by new
                {
                    container.Id,
                    container.Status,
                    container.CompanyId,
                    container.ContainerNumber,
                    container.Bags,
                    container.StuffingWeightKg,
                    container.SealNumber,
                    container.WeighbridgeWeightKg,
                    container.BookingNumber

                }
                into grp
                where grp.Count() == 1
                select new
                {
                    grp.Key.Id,
                    grp.Key.Status,
                    grp.Key.CompanyId,
                    grp.Key.ContainerNumber,
                    grp.Key.Bags,
                    grp.Key.StuffingWeightKg,
                    grp.Key.SealNumber,
                    grp.Key.WeighbridgeWeightKg,
                    grp.Key.BookingNumber
                };

            Index(x => x.Id, FieldIndexing.Default);
            Index(x => x.Status, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);
            

            // StoreAllFields(FieldStorage.Yes);
        }

        public class Result
        {
            public ContainerStatus Status { get; set; }
            public string CompanyId { get; set; }
            public string Id { get; set; }
            public string ContainerNumber { get; set; }
            public double Bags { get; set; }
            public double StuffingWeightKg { get; set; }
            public string SealNumber { get; set; }
            public string BookingNumber { get; set; }
            public double WeighbridgeWeightKg { get; set; }

            public override string ToString()
            {
                return $"Status: {Status}, CompanyId: {CompanyId}, ContainerId: {Id}";
            }
        }
    }
}