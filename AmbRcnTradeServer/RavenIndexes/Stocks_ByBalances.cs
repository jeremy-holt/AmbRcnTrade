using System.Linq;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents.Indexes;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Stocks_ByBalances : AbstractIndexCreationTask<Stock, Stocks_ByBalances.Result>
    {
        public class Result
        {
            public long LotNo { get; set; }
            public string CompanyId { get; set; }
            public string LocationId { get; set; }
            public double BagsIn { get; set; }
            public double BagsOut { get; set; }
            public double WeightKgIn { get; set; }
            public double WeightKgOut { get; set; }
            public StockInfo StockIn { get; set; } = new();
            public StockInfo StockOut { get; set; } = new();
        }

        public Stocks_ByBalances()
        {
            Map = stocks => from c in stocks
                select new
                {
                    c.LotNo,
                    c.CompanyId,
                    c.LocationId,
                    BagsIn = c.Bags > 0 ? c.Bags : 0,
                    WeightKgIn = c.WeightKg > 0 ? c.WeightKg : 0,
                    BagsOut = c.Bags < 0 ? c.Bags : 0,
                    WeightKgOut = c.WeightKg < 0 ? c.WeightKg : 0,
                    StockIn = new StockInfo(),
                    StockOut = new StockInfo()
                };
            Reduce = results => from c in results
                group c by new {c.LotNo, c.CompanyId, c.LocationId}
                into grp
                select new
                {
                    grp.Key.LotNo,
                    grp.Key.CompanyId,
                    grp.Key.LocationId,
                    BagsIn = 0.0,
                    WeightKgIn = 0.0,
                    BagsOut = 0.0,
                    WeightKgOut = 0.0,
                    StockIn = new StockInfo {Bags = grp.Sum(x => x.BagsIn), WeightKg = grp.Sum(x => x.WeightKgIn)},
                    StockOut = new StockInfo {Bags = grp.Sum(x => x.BagsOut), WeightKg = grp.Sum(x => x.WeightKgOut)},
                };

            Index(x => x.LotNo, FieldIndexing.Default);
            Index(x => x.LocationId, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}