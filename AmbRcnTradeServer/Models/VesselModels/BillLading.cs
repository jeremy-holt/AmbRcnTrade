using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Models.ContainerModels;
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