using System;
using System.Collections.Generic;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Models.InspectionModels;
using Newtonsoft.Json;

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
        public List<string> InspectionIds { get; set; } = new();
        public DateTime? StockOutDate { get; set; }
        public bool IsStockIn { get; set; }
        public Analysis AnalysisResult { get; set; } = new();
        [JsonIgnore]
        public List<Inspection> Inspections { get; set; } = new();

        public string Origin { get; set; }
    }

    
}