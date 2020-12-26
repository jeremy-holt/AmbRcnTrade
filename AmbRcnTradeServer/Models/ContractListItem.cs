using System;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models
{
    public class ContractListItem
    {
        public string Id { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public ListItem Seller { get; set; }
        public ListItem Buyer { get; set; }
        public ListItem Broker { get; set; }
        public string ContainerNumber { get; set; }
        public string VesselName { get; set; }
        public DateTime? VesselEta { get; set; }
        public string BlNumber { get; set; }
        public DateTime? BlDate { get; set; }
        public ContainerStatus Status { get; set; }
        public string CompanyId { get; set; }
    }
}