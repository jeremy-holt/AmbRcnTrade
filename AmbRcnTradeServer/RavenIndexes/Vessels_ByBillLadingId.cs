using System.Linq;
using AmbRcnTradeServer.Models.VesselModels;
using Raven.Client.Documents.Indexes;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Vessels_ByBillLadingId : AbstractIndexCreationTask<Vessel, Vessels_ByBillLadingId.Result>
    {
        public class Result
        {
            public string VesselId { get; set; }
            public string BillLadingId { get; set; }
        }

        public Vessels_ByBillLadingId()
        {
            Map = vessels => from vessel in vessels
                from billLadingId in vessel.BillLadingIds
                select new
                {
                    VesselId = vessel.Id,
                    BillLadingId = billLadingId
                };

            Index(x => x.VesselId, FieldIndexing.Default);
            Index(x => x.BillLadingId, FieldIndexing.Default);
            
            StoreAllFields(FieldStorage.Yes);
        }
    }
}