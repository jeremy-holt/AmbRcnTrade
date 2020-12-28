using System;
using System.Collections.Generic;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class Inspection : IEntityCompany
    {
        public DateTime InspectionDate { get; set; }
        public string Inspector { get; set; }
        public string LotNo { get; set; }
        public string Location { get; set; }
        public string TruckPlate { get; set; }
        public double ApproxWeight { get; set; }
        public double Bags { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public List<Analysis> Analyses { get; init; } = new();
        public Approval Approved { get; set; }
        public string SupplierId { get; set; }
        public Analysis AnalysisResult { get; set; } = new();
    }
}