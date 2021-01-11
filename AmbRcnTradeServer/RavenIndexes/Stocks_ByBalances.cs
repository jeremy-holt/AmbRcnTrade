using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
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
                let supplier = LoadDocument<Customer>(c.SupplierId)
                select new
                {
                    c.LotNo,
                    c.CompanyId,
                    c.LocationId,
                    BagsIn = c.IsStockIn ? c.Bags : 0,
                    BagsOut = c.IsStockIn ? 0 : c.Bags,
                    WeightKgIn = c.IsStockIn ? c.WeightKg : 0,
                    WeightKgOut = c.IsStockIn ? 0 : c.WeightKg,
                    LocationName = location.Name,
                    c.AnalysisResult,
                    c.InspectionId,
                    AnalysisResults = new List<object>(),
                    InspectionIds = new List<string>(),
                    c.SupplierId,
                    SupplierName = supplier.Name,
                    c.ZeroedStock
                };
            Reduce = results => from c in results
                group c by new {c.CompanyId, c.LocationId, c.LocationName, c.LotNo, c.SupplierId, c.SupplierName, c.ZeroedStock}
                into grp
                select new
                {
                    grp.Key.LotNo,
                    grp.Key.CompanyId,
                    grp.Key.LocationId,
                    grp.Key.LocationName,
                    grp.Key.SupplierId,
                    grp.Key.SupplierName,
                    grp.Key.ZeroedStock,
                    BagsIn = grp.Sum(x => x.BagsIn),
                    BagsOut = grp.Sum(x => x.BagsOut),
                    WeightKgIn = grp.Sum(x => x.WeightKgIn),
                    WeightKgOut = grp.Sum(x => x.WeightKgOut),
                    AnalysisResult = default(object),
                    InspectionId = default(string),
                    AnalysisResults = grp.Select(x => x.AnalysisResult),
                    InspectionIds = grp.Select(x => x.InspectionId).ToList()
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
            public string LocationName { get; set; }
            public string SupplierId { get; set; }
            public string SupplierName { get; set; }
            public double BagsIn { get; set; }
            public double BagsOut { get; set; }
            public double WeightKgIn { get; set; }
            public double WeightKgOut { get; set; }
            public bool ZeroedStock { get; set; }
            public AnalysisResult AnalysisResult { get; set; }
            public string InspectionId { get; set; }
            public IEnumerable<AnalysisResult> AnalysisResults { get; set; }
            public IEnumerable<string> InspectionIds { get; set; }
        }
    }
}