using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockManagementModels
{
    public class RemoveInspectionFromStockRequest
    {
        public string StockId { get; set; }
        public string InspectionId { get; set; }
    }
}