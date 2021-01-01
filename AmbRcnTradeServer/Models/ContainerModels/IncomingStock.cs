using System;
using System.Collections.Generic;
using AmbRcnTradeServer.Models.InspectionModels;

namespace AmbRcnTradeServer.Models.ContainerModels
{
    public class IncomingStock
    {
        public string StockId { get; set; }
        public double Bags { get; set; }
        public double WeightKg { get; set; }
        public long LotNo { get; set; }
        public string InspectionId { get; set; }
        public AnalysisResult AnalysisResult { get; set; }
        public string SupplierId { get; set; }
        public string LocationId { get; set; }
        public string Origin { get; set; }
        public string CompanyId { get; set; }
    }

    public class StuffingRequest
    {
        public string ContainerId { get; set; }
        public DateTime StuffingDate { get; set; }
        public List<IncomingStock> IncomingStocks { get; set; } = new List<IncomingStock>();
    }

    public class OutgoingStock
    {
        public string StockId { get; set; }
    }
}