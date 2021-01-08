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

            var stocks = await _session.Query<Stock>().Where(c => c.Id.In(purchase.PurchaseDetails.SelectMany(x => x.StockIds))).ToListAsync();
            
            foreach (var detail in purchase.PurchaseDetails)
            {
                var weightKg = stocks.Where(c => c.Id.In(detail.StockIds)).Sum(x => x.WeightKg);
                detail.Value = detail.PricePerKg * weightKg;
                detail.ValueUsd = detail.Value / detail.ExchangeRate;
            }

            purchase.Value = purchase.PurchaseDetails.Sum(x => x.Value);
            purchase.ValueUsd = purchase.PurchaseDetails.Sum(x => x.ValueUsd);
            
            await _session.StoreAsync(purchase);
            return new ServerResponse<Purchase>(purchase, "Saved");
        }

        public async Task<Purchase> Load(string id)
        {
            Debug.WriteLine(_session.Advanced.NumberOfRequests);

            var purchase = await _session
                .LoadAsync<Purchase>(id);

            var stockIds = purchase.PurchaseDetails.SelectMany(x => x.StockIds).ToList();
            var stocks = await _stockService.LoadStockList(purchase.CompanyId, stockIds);

            foreach (var item in purchase.PurchaseDetails)
                item.Stocks = stocks.Where(c => c.IsStockIn && c.StockId.In(item.StockIds)).ToList();

            return purchase;
        }

        public async Task<List<PurchaseListItem>> LoadList(string companyId, string supplierId)
        {
            var query = _session.Query<Purchase>()
                .Include(c => c.SupplierId)
                .Include(c => c.PurchaseDetails.SelectMany(x => x.StockIds));

            if (companyId.IsNotNullOrEmpty()) 
                query = query.Where(c => c.CompanyId == companyId);
            
            if (supplierId.IsNotNullOrEmpty())
                query = query.Where(c => c.SupplierId == supplierId);

            var purchases = await query.ToListAsync();

            var suppliers = await _session.LoadListFromMultipleIdsAsync<Customer>(purchases.Select(c => c.SupplierId));

            var stockIds = purchases.SelectMany(c => c.PurchaseDetails.SelectMany(x => x.StockIds)).Distinct().ToList();

            var stocksDictionary = await _session
                .Include<Stock>(c => c.InspectionId)
                .LoadAsync<Stock>(stockIds);
            var stocks = stocksDictionary.Where(c => c.Value != null && c.Value.IsStockIn).Select(c => c.Value).ToList();
            
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
                    SupplierName = suppliers.FirstOrDefault(c => c.Id == purchase.SupplierId)?.Name,
                    Value = purchase.Value,
                    ValueUsd = purchase.ValueUsd
                };

                foreach (var detail in purchase.PurchaseDetails)
                {
                    var detailListItem = new PurchaseDetailListItem
                    {
                        Currency = detail.Currency,
                        PriceAgreedDate = detail.PriceAgreedDate,
                        PricePerKg = detail.PricePerKg,
                        StockIds = detail.StockIds,
                        Value = detail.Value,
                        ValueUsd = detail.ValueUsd
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

                    detailListItem.BagsIn = detailListItem.Stocks.Sum(x => x.BagsIn);
                    detailListItem.BagsOut = detailListItem.Stocks.Sum(x => x.BagsOut);

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