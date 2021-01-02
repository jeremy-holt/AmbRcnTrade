using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.ContainerModels
{
    public class AvailableContainerItem
    {
        public ContainerStatus Status { get; set; }
        public string ContainerNumber { get; set; }
        public string BookingNumber { get; set; }
        public double Bags { get; set; }
        public double StockWeightKg { get; set; }
        public string ContainerId { get; set; }
        public bool IsOverweight { get; set; }
    }
}