using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockManagementModels
{
    public class MovedInspectionResult
    {
        public MovedInspectionResult(string stockId)
        {
            StockId = stockId;
        }

        public string StockId { get; set; }
    }
}