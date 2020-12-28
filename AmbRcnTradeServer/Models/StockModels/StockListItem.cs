﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class StockListItem
    {
        public StockInfo StockIn { get; set; }
        public StockInfo StockOut { get; set; }
        public Analysis AnalysisResult { get; set; }
        public string LocationId { get; set; }
        public long LotNo { get; set; }
        public string StockId { get; set; }
        public bool IsStockIn { get; set; }
        public DateTime Date { get; set; }
        public string SupplierNames { get; set; }
        public string LocationName { get; set; }
        public string Origin { get; set; }
    }
}