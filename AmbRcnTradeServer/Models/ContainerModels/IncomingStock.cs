using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.ContainerModels
{
    public class IncomingStock
    {
        public double Bags { get; set; }

        public double WeightKg { get; set; }
        public long LotNo { get; set; }
        public List<IncomingStockItem> StockIds { get; set; } = new List<IncomingStockItem>();
        public DateTime StuffingDate { get; set; }
    }

    public class StuffingRequest
    {
        public string ContainerId { get; set; }
        public DateTime StuffingDate { get; set; }
        public StockBalance StockBalance { get; set; }
        public double Bags { get; set; }
        public double WeightKg { get; set; }
    }

    public class OutgoingStock
    {
        public string StockId { get; set; }
    }

    public class IncomingStockItem
    {
        public IncomingStockItem(string stockId, bool isStockIn)
        {
            StockId = stockId;
            IsStockIn = isStockIn;
        }
        public string StockId { get; set; }
        public bool IsStockIn { get; set; }
    }
}