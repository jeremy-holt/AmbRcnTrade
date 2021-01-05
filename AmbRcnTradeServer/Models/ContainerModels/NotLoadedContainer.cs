using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

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