using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models
{
    public class ContractQueryParameters
    {
        public string AppUserId { get; set; }
        public string CompanyId { get; set; }
        public string SellerId { get; set; }
        public string BuyerId { get; set; }
        public ContainerStatus? ContainerStatus { get; set; }
    }
}