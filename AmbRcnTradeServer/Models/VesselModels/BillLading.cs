using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.DraftBillLadingModels;
using AutoMapper;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.VesselModels
{
    public class BillLading : IEntityCompany
    {
        public string NotifyParty1Id { get; set; }
        public string NotifyParty2Id { get; set; }
        public string ConsigneeId { get; set; }
        public string BlBodyText { get; set; }
        public string ShipperId { get; set; }
        public bool FreightPrepaid { get; set; }
        public int ContainersOnBoard { get; set; }
        public DateTime? BlDate { get; set; }
        public string BlNumber { get; set; }
        public List<string> ContainerIds { get; set; } = new();
        public string VesselId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        
        public string OwnReferences { get; set; }
        public string ShipperReference { get; set; }
        public string ConsigneeReference { get; set; }
        public string DestinationAgentId { get; set; }
        
        public string ShippingMarks { get; set; }
 
        public string ForwarderReference { get; set; }
        public string PortOfDestinationId { get; set; }
        public string PortOfLoadingId { get; set; }
        public string PortOfDestinationName { get; set; }
        
        
        public double? NumberBags { get; set; }
        public string NumberPackagesText { get; set; }
        
        public double? NettWeightKg { get; set; }
        public double? GrossWeightKg { get; set; }
        public string NettWeightKgText { get; set; }
        
        public string GrossWeightKgText { get; set; }
        public string VgmWeightKgText { get; set; }
        
        public string OceanFreight { get; set; }
        public string OceanFreightPaidBy { get; set; }
        public string FreightOriginCharges { get; set; }
        public string FreightOriginChargesPaidBy { get; set; }
        public string FreightDestinationCharge { get; set; }
        public string FreightDestinationChargePaidBy { get; set; }
        public string ProductDescription { get; set; } = "IVORY COAST ORIGIN 2020 SEASON";
        public CargoDescription PreCargoDescription { get; set; }
        public Teu Teu { get; set; }
        public List<Document> Documents { get; set; } = new List<Document>();
        public string DeclarationNumber { get; set; }
    }

    public class BillLadingDto : BillLading, IEntityDto
    {
        [IgnoreMap]
        public List<Container> Containers { get; set; } = new();

        


        public void Validate()
        {
            if (VesselId.IsNullOrEmpty())
                throw new InvalidOperationException("Cannot create a Bill of Lading without the Vessel Id");
        }
    }
}