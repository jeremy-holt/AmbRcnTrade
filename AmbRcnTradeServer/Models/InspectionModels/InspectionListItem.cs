using System;
using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class InspectionListItem
    {
        public Approval Approved { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectionId { get; set; }
        public string Location { get; set; }
        public string LotNo { get; set; }
        public string Inspector { get; set; }
        public double Bags { get; set; }
        public string TruckPlate { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
    }
}