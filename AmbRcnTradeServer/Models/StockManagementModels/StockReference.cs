using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockManagementModels
{
    public class StockReference
    {
        public StockReference(string stockId, double bags)
        {
            StockId = stockId;
            Bags = bags;
        }

        public string StockId { get; set; }
        public double Bags { get; set; }
    }
}