using System;

namespace AmbRcnTradeServer.Models.StockManagementModels
{
    public class MoveInspectionToStockRequest
    {
        public string InspectionId { get; set; }
        public double Bags { get; set; }
        public DateTime Date { get; set; }
        public string LocationId { get; set; }
        public long LotNo { get; set; }
    }
}