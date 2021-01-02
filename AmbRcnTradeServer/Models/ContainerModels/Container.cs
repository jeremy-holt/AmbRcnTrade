using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Constants;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.ContainerModels
{
    public class Container : IEntityCompany
    {
        public string ContainerNumber { get; set; }
        public string SealNumber { get; set; }
        public string BookingNumber { get; set; }
        public double Bags { get; set; }
        public double WeighbridgeWeightKg { get; set; } 
        public ContainerStatus Status { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public DateTime? StuffingDate { get; set; }
        public DateTime? DispatchDate { get; set; }
        public double StuffingWeightKg { get; set; }
        public List<IncomingStock> IncomingStocks { get; set; } = new();
        public double NettWeightKg { get; set; }
    }
}