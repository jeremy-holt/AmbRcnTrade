using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Models.StockManagementModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class Inspection : IEntityCompany
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public DateTime InspectionDate { get; set; }
        public double Bags { get; set; }
        public string Inspector { get; set; }
        public string LotNo { get; set; }
        public string Location { get; set; }
        public string SupplierId { set; get; }
        public string TruckPlate { get; set; }
        public List<StockReference> StockReferences { get; set; } = new();
        public List<Analysis> Analyses { get; init; } = new();
        public Analysis AnalysisResult { get; set; } = new();
    }
}