using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockManagementModels
{
    public class MovedInspectionResult
    {
        public MovedInspectionResult(string stockId, Inspection inspection)
        {
            StockId = stockId;
            Inspection = inspection;
        }

        public string StockId { get; set; }
        public Inspection Inspection { get; }
    }
}