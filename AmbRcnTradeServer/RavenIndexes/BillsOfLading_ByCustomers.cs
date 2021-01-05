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
    public class BillsOfLading_ByCustomers : AbstractIndexCreationTask<BillLading, BillLadingListItem>
    {
        public BillsOfLading_ByCustomers()
        {
            Map = billsOfLading => from c in billsOfLading
                let shipper = LoadDocument<Customer>(c.ShipperId)
                let consignee = LoadDocument<Customer>(c.ConsigneeId)
                let notifyParty1 = LoadDocument<Customer>(c.NotifyParty1Id)
                let notifyParty2 = LoadDocument<Customer>(c.NotifyParty2Id)
                select new BillLadingListItem
                {
                    Id = c.Id,
                    BlDate = c.BlDate,
                    BlNumber = c.BlNumber,
                    ConsigneeName = consignee.Name,
                    ShipperName = shipper.Name,
                    ContainersOnBoard = c.ContainersOnBoard,
                    NotifyParty1Name = notifyParty1.Name,
                    NotifyParty2Name = notifyParty2.Name,
                    CompanyId = c.CompanyId,
                    VesselId = c.VesselId
                };

            Index(x => x.BlDate, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}