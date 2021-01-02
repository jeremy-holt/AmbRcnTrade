using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Stocks_ByBalances : AbstractIndexCreationTask<Stock, Stocks_ByBalances.Result>
    {
        public Stocks_ByBalances()
        {
            Map = stocks => from c in stocks
                let location = LoadDocument<Customer>(c.LocationId)
                select new
                {
                    c.LotNo,
                    c.CompanyId,
                    c.LocationId,
                    BagsIn = c.Bags > 0 ? c.Bags : 0,
                    BagsOut = c.Bags < 0 ? c.Bags : 0,
                    LocationName = location.Name
                };
            Reduce = results => from c in results
                group c by new {c.CompanyId, c.LocationId, c.LocationName, c.LotNo}
                into grp
                select new
                {
                    grp.Key.LotNo,
                    grp.Key.CompanyId,
                    grp.Key.LocationId,
                    BagsIn = grp.Sum(x => x.BagsIn),
                    BagsOut = grp.Sum(x => x.BagsOut),
                    grp.Key.LocationName
                };

            Index(x => x.LotNo, FieldIndexing.Default);
            Index(x => x.LocationId, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }

        public class Result
        {
            public long LotNo { get; set; }
            public string CompanyId { get; set; }
            public string LocationId { get; set; }
            public double BagsIn { get; set; }
            public double BagsOut { get; set; }
            public string LocationName { get; set; }
        }
    }
}