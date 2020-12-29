using System;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.PurchaseModels
{
    public class PurchaseListItem
    {
        public string SupplierName { get; set; }
        public string Id { get; set; }
        public long PurchaseNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string SupplierId { get; set; }
        public StockInfo StockIn { get; set; }
        public StockInfo StockOut { get; set; }
        public StockInfo StockBalance { get; set; }
    }
}