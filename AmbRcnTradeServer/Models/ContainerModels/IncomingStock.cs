using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
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
        public List<IncomingStockItem> StockIds { get; set; } = new();
        public DateTime StuffingDate { get; set; }
        public double Kor { get; set; }

        public override string ToString()
        {
            return $"Bags: {Bags}, WeightKg: {WeightKg}, LotNo: {LotNo}";
        }
    }

    public class StuffingRequest
    {
        public string ContainerId { get; set; }
        public DateTime StuffingDate { get; set; }
        public StockBalance StockBalance { get; set; }
        public double Bags { get; set; }
        public double WeightKg { get; set; }
        public ContainerStatus Status { get; set; }
    }

    public class OutgoingStock
    {
        public string StockId { get; set; }

        public override string ToString()
        {
            return $"StockId: {StockId}";
        }
    }

    public class IncomingStockItem
    {
        public IncomingStockItem(string stockId, bool isStockIn)
        {
            StockId = stockId;
            IsStockIn = isStockIn;
        }

        public IncomingStockItem() { }
        public string StockId { get; set; }
        public bool IsStockIn { get; set; }

        public override string ToString()
        {
            return $"StockId: {StockId}, IsStockIn: {IsStockIn}";
        }
    }
}