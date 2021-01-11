using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Interfaces;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class Stock : IEntityCompany, IAnalysisResult
    {
        public string LocationId { get; set; }
        public bool IsStockIn { get; set; }
        public DateTime? StockInDate { get; set; }
        public DateTime? StockOutDate { get; set; }
        public double Bags { get; set; }
        public double WeightKg { get; set; }
        public long LotNo { get; set; }
        public string InspectionId { get; set; }
        public string Origin { get; set; }
        public string SupplierId { get; set; }
        public List<StuffingRecord> StuffingRecords { get; set; } = new();
        public AnalysisResult AnalysisResult { get; set; }
        public string CompanyId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public bool ZeroedStock { get; set; }
    }
}