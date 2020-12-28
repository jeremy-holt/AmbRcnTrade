using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class InspectionQueryParams
    {
        public string CompanyId { get; set; }
        public Approval? Approved { get; set; }
    }

 
}