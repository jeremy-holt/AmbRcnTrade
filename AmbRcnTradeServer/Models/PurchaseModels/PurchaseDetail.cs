using System;
using System.Collections.Generic;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.StockModels;
using Newtonsoft.Json;

namespace AmbRcnTradeServer.Models.PurchaseModels
{
    public class PurchaseDetail
    {
        public List<string> StockIds { get; init; } = new();
        public double ExchangeRate { get; set; }
        public Currency Currency { get; set; }
        public double PricePerKg { get; set; }
        public DateTime Date { get; set; }
        [JsonIgnore] public List<Stock> Stocks { get; set; } = new();
    }
}