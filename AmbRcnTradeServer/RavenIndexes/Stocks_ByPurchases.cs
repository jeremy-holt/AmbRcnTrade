using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.PurchaseModels;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Stocks_ByPurchases : AbstractMultiMapIndexCreationTask<StockListItem>
    {
        public Stocks_ByPurchases()
        {
            AddMap<Purchase>(purchases => from p in purchases
                from detail in p.PurchaseDetails
                from stockId in detail.StockIds
                let stock = LoadDocument<Stock>(stockId)
                let location = LoadDocument<Customer>(stock.LocationId)
                let supplier = LoadDocument<Customer>(stock.SupplierId)
                let inspection = LoadDocument<Inspection>(stock.InspectionId)
                select new StockListItem
                {
                    CompanyId = p.CompanyId,
                    PurchaseId = p.Id,
                    StockId = stockId,
                    LocationId = stock.LocationId,
                    LocationName = location.Name,
                    SupplierId = stock.SupplierId,
                    SupplierName = supplier.Name,
                    LotNo = stock.LotNo,
                    BagsIn = stock.IsStockIn ? stock.Bags : 0,
                    BagsOut = stock.IsStockIn ? 0 : stock.Bags,
                    AnalysisResult = inspection.AnalysisResult,
                    IsStockIn = stock.IsStockIn,
                    Origin = stock.Origin,
                    InspectionDate = inspection.InspectionDate,
                    InspectionId = stock.InspectionId,
                    StockDate = stock.IsStockIn ? stock.StockInDate : stock.StockOutDate
                });

            AddMap<Stock>(stocks => from stock in stocks
                let location = LoadDocument<Customer>(stock.LocationId)
                let supplier = LoadDocument<Customer>(stock.SupplierId)
                let inspection = LoadDocument<Inspection>(stock.InspectionId)
                select new StockListItem
                {
                    CompanyId = stock.CompanyId,
                    PurchaseId = default,
                    StockId = stock.Id,
                    LocationId = stock.LocationId,
                    LocationName = location.Name,
                    SupplierId = stock.SupplierId,
                    SupplierName = supplier.Name,
                    LotNo = stock.LotNo,
                    BagsIn = stock.IsStockIn ? stock.Bags : 0,
                    BagsOut = stock.IsStockIn ? 0 : stock.Bags,
                    AnalysisResult = inspection.AnalysisResult,
                    IsStockIn = stock.IsStockIn,
                    Origin = stock.Origin,
                    InspectionId = stock.InspectionId,
                    InspectionDate = inspection.InspectionDate,
                    StockDate = stock.IsStockIn ? stock.StockInDate : stock.StockOutDate
                }
            );

            Index(x => x.StockId, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);
            Index(x => x.IsStockIn, FieldIndexing.Default);
            Index(x => x.LotNo, FieldIndexing.Default);
            Index(x => x.PurchaseId, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}