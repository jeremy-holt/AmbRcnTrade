using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Models.InspectionModels;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class Stock : IEntityCompany
    {
        public string LocationId { get; set; }
        public DateTime? StockInDate { get; set; }
        public DateTime? StockOutDate { get; set; }
        public long LotNo { get; set; }
        public double Bags { get; set; }
        public double WeightKg { get; set; }
        public List<string> InspectionIds { get; set; } = new();
        public bool IsStockIn { get; set; }

        [JsonIgnore]
        public List<Inspection> Inspections { get; set; } = new();

        public string Origin { get; set; }
        public string SupplierId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
    }
}