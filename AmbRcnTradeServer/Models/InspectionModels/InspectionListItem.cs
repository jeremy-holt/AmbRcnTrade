using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class InspectionListItem
    {
        public Approval Approved { get; set; }
        public DateTime InspectionDate { get; set; }
        public string Id { get; set; }
        public string Location { get; set; }
        public string LotNo { get; set; }
        public string Inspector { get; set; }
        public double Bags { get; set; }
        public string TruckPlate { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public double Kor { get; set; }
        public double Count { get; set; }
        public double Moisture { get; set; }
        public double RejectsPct { get; set; }
        public List<StockReference> StockReferences { get; set; } = new();
        public int StockAllocations { get; set; }
        public double UnallocatedBags { get; set; }
        public double UnallocatedWeightKg { get; set; }
        public double WeightKg { get; set; }
    }
}