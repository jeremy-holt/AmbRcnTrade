﻿using AmbRcnTradeServer.Models.InspectionModels;

namespace AmbRcnTradeServer.Models.StockManagementModels
{
    public class UnAllocatedStock
    {
        public string StockId { get; set; }
        public string CompanyId { get; set; }
        public bool IsStockIn { get; set; }
        public string LocationName { get; set; }
        public string SupplierName { get; set; }
        public long LotNo { get; set; }
        public double Bags { get; set; }
        public AnalysisResult AnalysisResult { get; set; }
        public string PurchaseId { get; set; }
    }
}