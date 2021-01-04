using System;

namespace AmbRcnTradeServer.Models.VesselModels
{
    public class VesselListItem
    {
        public string Id { get; set; }
        public string VesselName { get; set; }
        public DateTime? Eta { get; set; }
        public int ContainersOnBoard { get; set; }
        public DateTime? BlDate { get; set; }
        public string BlNumber { get; set; }
        public string ShippingCompany { get; set; }
        public string ForwardingAgent { get; set; }
    }
}