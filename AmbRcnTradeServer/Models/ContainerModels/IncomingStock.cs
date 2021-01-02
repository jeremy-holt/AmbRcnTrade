using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.ContainerModels
{
    public class IncomingStock
    {
        public string StockId { get; set; }
        public double Bags { get; set; }

        public double WeightKg { get; set; }
    }

    public class StuffingRequest
    {
        public string ContainerId { get; set; }
        public DateTime StuffingDate { get; set; }
        public List<IncomingStock> IncomingStocks { get; set; } = new();
    }

    public class OutgoingStock
    {
        public string StockId { get; set; }
    }
}