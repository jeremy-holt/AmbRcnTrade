using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.PurchaseModels;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IPurchaseService
    {
        Task<ServerResponse<Purchase>> Save(Purchase purchase);
        Task<Purchase> Load(string id);
        Task<List<PurchaseListItem>> LoadList(string companyId, string supplierId);
    }

    public class PurchaseService : IPurchaseService
    {
        private readonly ICounterService _counterService;
        private readonly IAsyncDocumentSession _session;

        public PurchaseService(IAsyncDocumentSession session, ICounterService counterService)
        {
            _session = session;
            _counterService = counterService;
        }

        public async Task<ServerResponse<Purchase>> Save(Purchase purchase)
        {
            if (purchase.Id.IsNullOrEmpty())
                purchase.PurchaseNumber = await _counterService.GetNextPurchaseNumber(purchase.CompanyId);

            await _session.StoreAsync(purchase);
            return new ServerResponse<Purchase>(purchase, "Saved");
        }

        public async Task<Purchase> Load(string id)
        {
            Debug.WriteLine(_session.Advanced.NumberOfRequests);

            var purchase = await _session
                .Include<Purchase>(c => c.PurchaseDetails.SelectMany(x => x.StockIds))
                .LoadAsync<Purchase>(id);

            var stocksDictionary = await _session
                .Include<Stock>(c => c.LocationId)
                .Include(c => c.SupplierId)
                .Include(c => c.InspectionId)
                .LoadAsync<Stock>(purchase.PurchaseDetails.SelectMany(x => x.StockIds));
            var stocks = stocksDictionary.Where(c => c.Value != null).Select(c => c.Value).ToList();

            var inspections = await _session.LoadListFromMultipleIdsAsync<Inspection>(stocks.Select(x => x.InspectionId));
            var locations = await _session.LoadListFromMultipleIdsAsync<Customer>(stocks.Select(x => x.LocationId));
            var suppliers = await _session.LoadListFromMultipleIdsAsync<Customer>(stocks.Select(c => c.SupplierId));

            foreach (var item in purchase.PurchaseDetails)
            {
                item.Stocks = stocks.Where(c => c.Id.In(item.StockIds)).ToList();
                foreach (var stock in item.Stocks)
                {
                    stock.Inspection = inspections.FirstOrDefault(c => c.Id == stock.InspectionId);
                    stock.LocationName = locations.FirstOrDefault(c => c.Id == stock.LocationId)?.Name;
                    stock.SupplierName = suppliers.FirstOrDefault(c => c.Id == stock.SupplierId)?.Name;
                    stock.AnalysisResult = stock.Inspection?.AnalysisResult;
                }
            }

            return purchase;
        }

        public async Task<List<PurchaseListItem>> LoadList(string companyId, string supplierId)
        {
            var query = _session.Query<Purchase>()
                .Include(c => c.SupplierId)
                .Include(c => c.PurchaseDetails.SelectMany(x => x.StockIds))
                .Where(c => c.CompanyId == companyId);

            if (supplierId.IsNotNullOrEmpty())
                query = query.Where(c => c.SupplierId == supplierId);

            var purchases = await query.ToListAsync();

            var suppliers = await _session.LoadListFromMultipleIdsAsync<Customer>(purchases.Select(c => c.SupplierId));

            var stockIds = purchases.SelectMany(c => c.PurchaseDetails.SelectMany(x => x.StockIds)).ToList();

            var stocksDictionary = await _session
                .Include<Stock>(c=>c.InspectionId)
                .LoadAsync<Stock>(stockIds);
            var stocks = stocksDictionary.Where(c => c.Value != null).Select(c => c.Value).ToList();

            var inspections = await _session.LoadListFromMultipleIdsAsync<Inspection>(stocks.Select(x => x.InspectionId));

            var purchaseList = new List<PurchaseListItem>();

            foreach (var purchase in purchases)
            {
                var purchaseListItem = new PurchaseListItem
                {
                    Id = purchase.Id,
                    PurchaseDate = purchase.PurchaseDate,
                    PurchaseNumber = purchase.PurchaseNumber,
                    QuantityMt = purchase.QuantityMt,
                    SupplierId = purchase.SupplierId,
                    SupplierName = suppliers.FirstOrDefault(c => c.Id == purchase.SupplierId)?.Name
                };

                foreach (var detail in purchase.PurchaseDetails)
                {
                    var detailListItem = new PurchaseDetailListItem
                    {
                        Currency = detail.Currency,
                        Date = detail.Date,
                        PricePerKg = detail.PricePerKg,
                        StockIds = detail.StockIds
                    };
                    var foundStocks = stocks.Where(c => c.Id.In(detail.StockIds)).ToList();
                    
                    detailListItem.Stocks = foundStocks.Select(c => new PurchaseDetailStockListItem
                    {
                        StockId = c.Id,
                        InspectionId = c.InspectionId,
                        IsStockIn = c.IsStockIn,
                        StockIn = c.IsStockIn ? new StockInfo(c.Bags, c.WeightKg) : new StockInfo(),
                        StockOut = c.IsStockIn ? new StockInfo() : new StockInfo(c.Bags, c.WeightKg),
                        AnalysisResult = inspections.FirstOrDefault(x=>x.Id==c.InspectionId)?.AnalysisResult
                    }).ToList();
                    
                    detailListItem.AnalysisResult = new AnalysisResult()
                    {
                        Kor = foundStocks.Average(c => c.AnalysisResult.Kor),
                        Count = foundStocks.Average(c => c.AnalysisResult.Count),
                        Moisture = foundStocks.Average(c => c.AnalysisResult.Moisture)
                    };
                    
                    foreach (var item in detailListItem.Stocks)
                    {
                        item.StockBalance = new StockInfo(item.StockIn.Bags - item.StockOut.Bags, item.StockIn.WeightKg - item.StockOut.WeightKg);
                    }

                    detailListItem.StockIn = new StockInfo(detailListItem.Stocks.Sum(x => x.StockIn.Bags), detailListItem.Stocks.Sum(x => x.StockIn.WeightKg));
                    detailListItem.StockOut = new StockInfo(detailListItem.Stocks.Sum(x => x.StockOut.Bags), detailListItem.Stocks.Sum(x => x.StockOut.WeightKg));
                    detailListItem.StockBalance = new StockInfo(detailListItem.StockIn.Bags - detailListItem.StockOut.Bags,
                        detailListItem.StockIn.WeightKg - detailListItem.StockOut.WeightKg);
                    
                    purchaseListItem.PurchaseDetails.Add(detailListItem);
                }

                var stockInBags = purchaseListItem.PurchaseDetails.SelectMany(c => c.Stocks).Sum(x => x.StockIn.Bags);
                var stockInWeightKg = purchaseListItem.PurchaseDetails.SelectMany(c => c.Stocks).Sum(x => x.StockIn.WeightKg);

                purchaseListItem.StockIn = new StockInfo(stockInBags, stockInWeightKg);

                var stockOutBags = purchaseListItem.PurchaseDetails.SelectMany(c => c.Stocks).Sum(x => x.StockOut.Bags);
                var stockOutWeightKg = purchaseListItem.PurchaseDetails.SelectMany(c => c.Stocks).Sum(x => x.StockOut.WeightKg);

                purchaseListItem.StockOut = new StockInfo(stockOutBags, stockOutWeightKg);

                var stockBalanceBags = new StockInfo(stockInBags - stockOutBags, stockInWeightKg - stockOutWeightKg);
                purchaseListItem.StockBalance = stockBalanceBags;

                purchaseList.Add(purchaseListItem);
            }

            return purchaseList;
        }
    }
}