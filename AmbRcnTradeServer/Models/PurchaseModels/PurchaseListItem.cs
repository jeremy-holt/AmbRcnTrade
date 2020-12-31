using System;
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
        public StockInfo StockIn { get; set; }
        public StockInfo StockOut { get; set; }
        public StockInfo StockBalance { get; set; }
        public double PricePerKg { get; set; }
        public Currency Currency { get; set; }
        public double ExchangeRate { get; set; }
        public double QuantityMt { get; set; }
    }
}