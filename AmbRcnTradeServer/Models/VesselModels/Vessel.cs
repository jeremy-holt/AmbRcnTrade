using System;
using System.Collections.Generic;
using AmberwoodCore.Interfaces;

namespace AmbRcnTradeServer.Models.VesselModels
{
    public class Vessel : IEntityCompany
    {
        public List<EtaHistory> EtaHistory { get; set; } = new();
        public string ShippingCompany { get; set; }
        public string ForwardingAgent { get; set; }
        public DateTime? BlDate { get; set; }
        public string BlNumber { get; set; }
        public List<string> ContainerIds { get; set; } = new();
        public int ContainersOnBoard { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public string NotifyParty1 { get; set; }
        public string NotifyParty2 { get; set; }
        public string Consignee { get; set; }
        public string BlBodyText { get; set; }
        public bool FreightPrepaid { get; set; }
    }
}