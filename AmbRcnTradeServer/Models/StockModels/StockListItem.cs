using System;
using System.Collections.Generic;
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
        public string CompanyId { get; set; }
        public string PurchaseId { get; set; }
        public string LocationId { get; set; }
        public double BagsIn { get; set; }
        public double BagsOut { get; set; }
        public long LotNo { get; set; }
        public string StockId { get; set; }
        public bool IsStockIn { get; set; }
        public DateTime? StockDate { get; set; }
        public string LocationName { get; set; }
        public string Origin { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public string InspectionId { get; set; }
        public DateTime? InspectionDate { get; set; }
        public AnalysisResult AnalysisResult { get; set; } = new();
        public double WeightKgIn { get; set; }
        public double WeightKgOut { get; set; }
        public List<StuffingRecord> StuffingRecords { get; set; } = new();

        public override string ToString()
        {
            return $"BagsIn: {BagsIn}, BagsOut: {BagsOut}, LotNo: {LotNo}, StockId: {StockId}, IsStockIn: {IsStockIn}";
        }
    }
}