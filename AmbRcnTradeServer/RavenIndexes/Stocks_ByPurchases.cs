using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.PurchaseModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Stocks_ByPurchases : AbstractIndexCreationTask<Purchase, Stocks_ByPurchases.Result>
    {
        public Stocks_ByPurchases()
        {
            Map = purchases => from p in purchases
                from detail in p.PurchaseDetails
                from stockId in detail.StockIds
                select new
                {
                    p.CompanyId,
                    StockId = stockId
                };
            
            Index(x => x.StockId, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);

            Store(x => x.StockId, FieldStorage.Yes);
        }

        public class Result
        {
            public string StockId { get; set; }
            public string CompanyId { get; set; }
        }
    }
}