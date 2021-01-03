using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.ContainerModels
{
    public class AvailableContainer
    {
        public ContainerStatus Status { get; set; }
        public string ContainerNumber { get; set; }
        public string BookingNumber { get; set; }
        public double Bags { get; set; }
        public double StockWeightKg { get; set; }
        public string Id { get; set; }
        public bool IsOverweight { get; set; }
    }
}