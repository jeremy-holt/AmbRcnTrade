using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.PurchaseModels
{
    public class PurchaseListItem
    {
        public string Id { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public long PurchaseNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public double QuantityMt { get; set; }
        public List<PurchaseDetailListItem> PurchaseDetails { get; set; } = new();
        public StockInfo StockIn { get; set; } = new();
        public StockInfo StockOut { get; set; } = new();
        public StockInfo StockBalance { get; set; } = new();
    }
}