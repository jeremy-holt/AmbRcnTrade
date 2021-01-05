// using System.Linq;
// using System.Threading.Tasks;
// using AmbRcnTradeServer.Models.DictionaryModels;
// using AmbRcnTradeServer.Models.InspectionModels;
// using AmbRcnTradeServer.Models.PurchaseModels;
// using AmbRcnTradeServer.Models.StockModels;
// using Raven.Client.Documents;
// using Raven.Client.Documents.Indexes;
// using Raven.Client.Documents.Linq;
// using Raven.Client.Documents.Session;
//
// namespace AmbRcnTradeServer.RavenIndexes
// {
//     public class Purchases_ById : AbstractIndexCreationTask<Purchase, PurchaseListItem>
//     {
//         public Purchases_ById()
//         {
//             Map = purchases => from p in purchases
//                 let supplier = LoadDocument<Customer>(p.SupplierId)
//                 let stocks = LoadDocument<Stock>(p.PurchaseDetails.SelectMany(x => x.StockIds))
//                 select new PurchaseListItem
//                 {
//                     Id = p.Id,
//                     CompanyId = p.CompanyId,
//                     PurchaseDate = p.PurchaseDate,
//                     SupplierId = p.SupplierId,
//                     SupplierName = supplier.Name,
//                     QuantityMt = p.QuantityMt,
//                     PurchaseNumber = p.PurchaseNumber,
//                     BagsIn = stocks.Where(c => c.IsStockIn).Sum(x => x.Bags),
//                     BagsOut = stocks.Where(c => !c.IsStockIn).Sum(x => x.Bags),
//                     PurchaseDetails = p.PurchaseDetails.Select(x => new PurchaseDetailListItem
//                     {
//                         PriceAgreedDate = x.PriceAgreedDate,
//                         Currency = x.Currency,
//                         PricePerKg = x.PricePerKg,
//                         AnalysisResult = new AnalysisResult
//                             {
//                                 Count = stocks.Where(c => c.Id.In(x.StockIds)).Average(an => an.AnalysisResult.Count),
//                                 Kor = stocks.Where(c => c.Id.In(x.StockIds)).Average(an => an.AnalysisResult.Kor),
//                                 Moisture = stocks.Where(c => c.Id.In(x.StockIds)).Average(an => an.AnalysisResult.Moisture)
//                             }
//                         // Stocks = stocks.Select(c => new PurchaseDetailStockListItem
//                         // {
//                         //     AnalysisResult = c.AnalysisResult,
//                         //     BagsIn = c.IsStockIn ? c.Bags : 0,
//                         //     BagsOut = c.IsStockIn ? 0 : c.Bags,
//                         //     InspectionId = c.InspectionId,
//                         //     StockId = c.Id,
//                         //     IsStockIn = c.IsStockIn
//                         // }).ToList()
//                     }).ToList()
//                 };
//
//             Index(x => x.SupplierId, FieldIndexing.Default);
//             Index(x => x.CompanyId, FieldIndexing.Default);
//
//             StoreAllFields(FieldStorage.Yes);
//         }
//     }
// }

using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;