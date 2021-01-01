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
        private readonly IInspectionService _inspectionService;
        private readonly IAsyncDocumentSession _session;
        private readonly IStockService _stockService;

        public PurchaseService(IAsyncDocumentSession session, ICounterService counterService, IInspectionService inspectionService, IStockService stockService)
        {
            _session = session;
            _counterService = counterService;
            _inspectionService = inspectionService;
            _stockService = stockService;
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
                // .Include<Purchase>(c => c.PurchaseDetails.SelectMany(x => x.StockIds))
                .LoadAsync<Purchase>(id);

            var stockIds = purchase.PurchaseDetails.SelectMany(x => x.StockIds).ToList();
            var stocks = await _stockService.LoadStockList(purchase.CompanyId, stockIds);

            // var stocksDictionary = await _session
            //     .Include<Stock>(c => c.LocationId)
            //     .Include(c => c.SupplierId)
            //     .LoadAsync<Stock>(purchase.PurchaseDetails.SelectMany(x => x.StockIds));
            // var stocks = stocksDictionary.Where(c => c.Value != null).Select(c => c.Value).ToList();
            //

            // var analysisResults = await _inspectionService.GetAnalysisResult(stocks.Select(x => x.InspectionId));
            // var locations = await _session.LoadListFromMultipleIdsAsync<Customer>(stocks.Select(x => x.LocationId));
            // var suppliers = await _session.LoadListFromMultipleIdsAsync<Customer>(stocks.Select(c => c.SupplierId));

            foreach (var item in purchase.PurchaseDetails)
            {
                item.Stocks = stocks.Where(c => c.StockId.In(item.StockIds)).ToList();
                // foreach (var stock in item.Stocks)
                // {
                //     stock.LocationName = locations.FirstOrDefault(c => c.Id == stock.LocationId)?.Name;
                //     stock.SupplierName = suppliers.FirstOrDefault(c => c.Id == stock.SupplierId)?.Name;
                //     stock.AnalysisResult = analysisResults.FirstOrDefault(c => c.InspectionId == stock.InspectionId);
                // }
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
                .Include<Stock>(c => c.InspectionId)
                .LoadAsync<Stock>(stockIds);
            var stocks = stocksDictionary.Where(c => c.Value != null).Select(c => c.Value).ToList();

            // var inspections = await _session.LoadListFromMultipleIdsAsync<Inspection>(stocks.Select(x => x.InspectionId));
            var analysisResults = await _inspectionService.GetAnalysisResult(stocks.Select(x => x.InspectionId));

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
                        Date = detail.PriceAgreedDate,
                        PricePerKg = detail.PricePerKg,
                        StockIds = detail.StockIds
                    };
                    var foundStocks = stocks.Where(c => c.Id.In(detail.StockIds)).ToList();

                    detailListItem.Stocks = foundStocks.Select(c => new PurchaseDetailStockListItem
                    {
                        StockId = c.Id,
                        InspectionId = c.InspectionId,
                        IsStockIn = c.IsStockIn,
                        BagsIn = c.IsStockIn ? c.Bags : 0,
                        BagsOut = c.IsStockIn ? 0 : c.Bags,
                        AnalysisResult = analysisResults.FirstOrDefault(x => x.InspectionId == c.InspectionId)
                    }).ToList();

                    detailListItem.AnalysisResult = new AnalysisResult
                    {
                        Kor = detailListItem.Stocks.Average(c => c.AnalysisResult.Kor),
                        Count = detailListItem.Stocks.Average(c => c.AnalysisResult.Count),
                        Moisture = detailListItem.Stocks.Average(c => c.AnalysisResult.Moisture)
                    };


                    foreach (var item in detailListItem.Stocks)
                    {
                        item.Balance = item.BagsIn - item.BagsOut;
                    }

                    detailListItem.BagsIn = detailListItem.Stocks.Sum(x => x.BagsIn);
                    detailListItem.BagsOut = detailListItem.Stocks.Sum(x => x.BagsOut);
                    detailListItem.Balance = detailListItem.BagsIn - detailListItem.BagsOut;

                    purchaseListItem.PurchaseDetails.Add(detailListItem);
                }

                purchaseListItem.BagsIn = purchaseListItem.PurchaseDetails.SelectMany(c => c.Stocks).Sum(x => x.BagsIn);
                purchaseListItem.BagsOut = purchaseListItem.PurchaseDetails.SelectMany(c => c.Stocks).Sum(x => x.BagsOut);
                purchaseListItem.Balance = purchaseListItem.BagsIn - purchaseListItem.BagsOut;

                purchaseList.Add(purchaseListItem);
            }

            return purchaseList;
        }
    }
}