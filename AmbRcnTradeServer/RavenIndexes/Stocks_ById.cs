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
    public class Stocks_ById : AbstractIndexCreationTask<Stock, StockListItem>
    {
        public Stocks_ById()
        {
            Map = stocks => from stock in stocks
                let location = LoadDocument<Customer>(stock.LocationId)
                let supplier = LoadDocument<Customer>(stock.SupplierId)
                let inspection = LoadDocument<Inspection>(stock.InspectionId)
                select new StockListItem
                {
                    CompanyId = stock.CompanyId,
                    StockId = stock.Id,
                    LocationId = stock.LocationId,
                    LocationName = location.Name,
                    SupplierId = stock.SupplierId,
                    SupplierName = supplier.Name,
                    LotNo = stock.LotNo,
                    BagsIn = stock.IsStockIn ? stock.Bags : 0,
                    BagsOut = stock.IsStockIn ? 0 : stock.Bags,
                    AnalysisResult = stock.AnalysisResult,
                    IsStockIn = stock.IsStockIn,
                    Origin = stock.Origin,
                    InspectionId = stock.InspectionId,
                    InspectionDate = inspection.InspectionDate,
                    StockDate = stock.IsStockIn?stock.StockInDate: stock.StockOutDate
                };
            
            Index(x => x.StockId, FieldIndexing.Default);
            Index(x => x.CompanyId, FieldIndexing.Default);
            Index(x => x.IsStockIn, FieldIndexing.Default);
            Index(x => x.LotNo, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}