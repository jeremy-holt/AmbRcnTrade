using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
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
}