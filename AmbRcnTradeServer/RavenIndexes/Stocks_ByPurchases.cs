using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.PurchaseModels;
using AmbRcnTradeServer.Models.StockManagementModels;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Stocks_ByPurchases : AbstractMultiMapIndexCreationTask<UnAllocatedStock>
    {
        public Stocks_ByPurchases()
        {
            AddMap<Purchase>(purchases => from p in purchases
                from detail in p.PurchaseDetails
                from stockId in detail.StockIds
                let stock = LoadDocument<Stock>(stockId)
                let location = LoadDocument<Customer>(stock.LocationId)
                let supplier = LoadDocument<Customer>(stock.SupplierId)
                select new
                {
                    p.CompanyId,
                    PurchaseId = p.Id,
                    StockId = stockId,
                    LocationName = location.Name,
                    SupplierName = supplier.Name,
                    stock.LotNo,
                    stock.Bags,
                    stock.AnalysisResult,
                    stock.IsStockIn
                    // IsStockIn = stock.StockInDate != null 
                });

            AddMap<Stock>(stocks => from s in stocks
                let location = LoadDocument<Customer>(s.LocationId)
                let supplier = LoadDocument<Customer>(s.SupplierId)
                select new
                {
                    s.CompanyId,
                    PurchaseId = default(string),
                    StockId = s.Id,
                    LocationName = location.Name,
                    SupplierName = supplier.Name,
                    s.LotNo,
                    s.Bags,
                    s.AnalysisResult,
                    s.IsStockIn
                }
            );

            Index(x => x.StockId, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);
            Index(x => x.IsStockIn, FieldIndexing.Default);
            Index(x => x.LotNo, FieldIndexing.Default);
            Index(x => x.PurchaseId, FieldIndexing.Default);

            // Store(x => x.StockId, FieldStorage.Yes);
            // Store(x=>x.IsStockIn, FieldStorage.Yes);
            StoreAllFields(FieldStorage.Yes);
        }

        // public class Result
        // {
        //     public string StockId { get; set; }
        //     public string CompanyId { get; set; }
        //     public bool IsStockIn { get; set; }
        //     public string LocationName { get; set; }
        //     public string SupplierName { get; set; }
        //     public long LotNo { get; set; }
        //     public double Bags { get; set; }
        //     public AnalysisResult AnalysisResult { get; set; }
        //     public string PurchaseId { get; set; }
        // }
    }
}