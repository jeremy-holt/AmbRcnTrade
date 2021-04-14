using System;
using System.Collections.Generic;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Models.ContainerModels;
using Newtonsoft.Json;

namespace AmbRcnTradeServer.Models.PackingListModels
{
    public class PackingList: IEntityCompany
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public List<string> ContainerIds { get; set; } = new();
        public string BookingNumber { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        [JsonIgnore]
        public List<Container> Containers { get; set; } = new();

        public string ShipperId { get; set; }
        public string FreightForwarderId { get; set; }
        public string WarehouseId { get; set; }
        public int OtNo { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string ContractNumber { get; set; }
        public string AmqNo { get; set; }
        public int NumberContainers { get; set; }
        public string VesselName { get; set; }
        public string CustomerId { get; set; }
        public string DestinationId { get; set; }
        public string DestinationCountry { get; set; }
        public string PackingListNumber { get; set; }
        public string Representative { get; set; }
    }
}