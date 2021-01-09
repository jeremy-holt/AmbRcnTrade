using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.VesselModels
{
    public class VesselListItem
    {
        public string Id { get; set; }
        public string VesselName { get; set; }
        public DateTime? Eta { get; set; }
        public string ShippingCompanyName { get; set; }
        public string ForwardingAgentName { get; set; }
        public int ContainersOnBoard { get; set; }
        public string CompanyId { get; set; }
        public string VoyageNumber { get; set; }
    }


    public class BillLadingListItem
    {
        public string Id { get; set; }

        public int ContainersOnBoard { get; set; }
        public DateTime? BlDate { get; set; }
        public string BlNumber { get; set; }
        public string ConsigneeName { get; set; }
        public string NotifyParty1Name { get; set; }
        public string NotifyParty2Name { get; set; }
        public string ShipperName { get; set; }
        public string CompanyId { get; set; }
        public string VesselId { get; set; }
    }
}