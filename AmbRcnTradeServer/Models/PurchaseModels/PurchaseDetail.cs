using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.StockModels;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.PurchaseModels
{
    public class PurchaseDetail
    {
        public List<string> StockIds { get; init; } = new();
        public double ExchangeRate { get; set; }
        public Currency Currency { get; set; }
        public double PricePerKg { get; set; }
        public DateTime Date { get; set; }

        [JsonIgnore]
        public List<Stock> Stocks { get; set; } = new();
    }

    public class PurchaseDetailListItem
    {
        public List<string> StockIds { get; init; } = new();
        public Currency Currency { get; set; }
        public double PricePerKg { get; set; }
        public DateTime Date { get; set; }
        public List<PurchaseDetailStockListItem> Stocks { get; set; } = new();
        public AnalysisResult AnalysisResult { get; set; } = new();
        public double BagsIn { get; set; }
        public double BagsOut { get; set; }
        public double Balance { get; set; }
    }

    public class PurchaseDetailStockListItem
    {
        public string StockId { get; set; }
        public string InspectionId { get; set; }
        public bool IsStockIn { get; set; }
        public AnalysisResult AnalysisResult { get; set; }
        public double BagsIn { get; set; }
        public double BagsOut { get; set; }
        public double Balance { get; set; }
    }
}