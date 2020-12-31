using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
                .Include(c=>c.InspectionId)
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

            var stocks = await _session.LoadListFromMultipleIdsAsync<Stock>(stockIds);

            var purchaseList = new List<PurchaseListItem>();

            foreach (var purchase in purchases)
            {
                foreach (var detail in purchase.PurchaseDetails)
                {
                    var purchaseItem = new PurchaseListItem
                    {
                        SupplierName = suppliers.FirstOrDefault(c => c.Id == purchase.SupplierId)?.Name,
                        Id = purchase.Id,
                        SupplierId = purchase.SupplierId,
                        PurchaseDate = purchase.PurchaseDate,
                        PurchaseNumber = purchase.PurchaseNumber,
                        PricePerKg = detail.PricePerKg,
                        Currency = detail.Currency,
                        ExchangeRate = detail.ExchangeRate,
                        QuantityMt = purchase.QuantityMt
                    };

                    var stockIdsForItem = purchase.PurchaseDetails.SelectMany(c => c.StockIds).ToList();
                    var stocksForItem = stocks.Where(c => c.Id.In(stockIdsForItem)).ToList();

                    purchaseItem.StockIn = new StockInfo(stocksForItem.Where(c => c.IsStockIn).Sum(c => c.Bags), stocksForItem.Where(c => c.IsStockIn).Sum(c => c.WeightKg));
                    purchaseItem.StockOut = new StockInfo(stocksForItem.Where(c => !c.IsStockIn).Sum(c => c.Bags), stocksForItem.Where(c => c.IsStockIn).Sum(c => c.WeightKg));
                    purchaseItem.StockBalance = new StockInfo(purchaseItem.StockIn.Bags + purchaseItem.StockOut.Bags,
                        purchaseItem.StockIn.WeightKg + purchaseItem.StockOut.WeightKg);

                    purchaseList.Add(purchaseItem);
                }
            }

            return purchaseList;
        }
    }
}