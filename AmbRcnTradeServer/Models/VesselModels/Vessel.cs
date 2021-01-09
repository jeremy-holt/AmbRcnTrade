using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AutoMapper;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.VesselModels
{
    public class Vessel : IEntityCompany
    {
        public int ContainersOnBoard { get; set; }
        public string ForwardingAgentId { get; set; }
        public string ShippingCompanyId { get; set; }
        public List<string> BillLadingIds { get; set; } = new();
        
  
        public string VesselName { get; set; }
        public string VoyageNumber { get; set; }
        public DateTime? Eta { get; set; }
        public string Notes { get; set; }
        public string CompanyId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string ServiceContract { get; set; }
        
        public string BookingNumber { get; set; }
    }

    public class VesselDto : Vessel, IEntityDto
    {
        [IgnoreMap]
        public List<BillLading> BillLadings { get; set; } = new();

        


        public void Validate() { }
    }
}