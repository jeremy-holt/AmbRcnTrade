namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class InspectionQueryParams
    {
        public string CompanyId { get; set; }
        public Approval? Approved { get; set; }
    }

    public enum Approval
    {
        Approved=1,
        Rejected=2,
    }
}