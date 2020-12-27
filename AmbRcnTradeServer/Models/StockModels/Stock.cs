using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AmberwoodCore.Interfaces;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class Stock : IEntityCompany
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public string LocationId { get; set; }
        public DateTime? StockInDate { get; set; }
        public long LotNo { get; set; }
        public double Bags { get; set; }
        public double WeightKg { get; set; }
        public List<string> InspectionIds { get; set; }
        public DateTime? StockOutDate { get; set; }
        
        [JsonIgnore]
        public bool IsStockIn => StockInDate != null;

        public Analysis AnalysisResult { get; set; }
    }

    
}