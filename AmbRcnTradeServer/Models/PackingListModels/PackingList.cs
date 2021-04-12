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
    }
}