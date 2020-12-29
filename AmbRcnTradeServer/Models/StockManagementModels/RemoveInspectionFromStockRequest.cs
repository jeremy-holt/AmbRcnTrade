namespace AmbRcnTradeServer.Models.StockManagementModels
{
    public class RemoveInspectionFromStockRequest
    {
        public string StockId { get; set; }
        public string InspectionId { get; set; }
    }
}