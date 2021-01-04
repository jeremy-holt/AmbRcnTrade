using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.ContainerModels
{
    public class NotLoadedContainer
    {
        public string ContainerId { get; set; }
        public string VesselId { get; set; }
        public string ContainerNumber { get; set; }
        public string BookingNumber { get; set; }
        public ContainerStatus Status { get; set; }
        public double Bags { get; set; }
        public string CompanyId { get; set; }

        public override string ToString()
        {
            return $"ContainerId: {ContainerId}, VesselId: {VesselId}, Status: {Status}";
        }
    }
}