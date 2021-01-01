using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class StockBalanceListItem
    {
        public long LotNo { get; set; }
        
        public double BagsIn { get; set; }
        public double BagsOut { get; set; }
        public double Balance => BagsIn - BagsOut;
        public bool IsStockZero => Balance > 1 || Balance < -1;
    }
}