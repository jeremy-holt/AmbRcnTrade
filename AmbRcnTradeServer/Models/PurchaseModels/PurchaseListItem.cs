using System;
using AmbRcnTradeServer.Models.StockModels;

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