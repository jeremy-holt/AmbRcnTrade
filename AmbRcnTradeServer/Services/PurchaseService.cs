using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
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
        Task<List<PurchaseListItem>> LoadList(string companyId);
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

            var purchase = await _session.Include<Purchase>(c => c.PurchaseDetails.SelectMany(x => x.StockIds)).LoadAsync<Purchase>(id);

            var stocks = await _session.LoadListFromMultipleIdsAsync<Stock>(purchase.PurchaseDetails.SelectMany(x => x.StockIds));

            foreach (var item in purchase.PurchaseDetails)
            {
                item.Stocks = stocks.Where(c => c.Id.In(item.StockIds)).ToList();
            }

            return purchase;
        }

        public async Task<List<PurchaseListItem>> LoadList(string companyId)
        {
            var purchases = await _session.Query<Purchase>()
                .Include(c => c.SupplierId)
                .Include(c => c.PurchaseDetails.SelectMany(x => x.StockIds))
                .Where(c => c.CompanyId == companyId)
                .ToListAsync();

            var suppliers = await _session.LoadListFromMultipleIdsAsync<Customer>(purchases.Select(c => c.SupplierId));

            var stockIds = purchases.SelectMany(c => c.PurchaseDetails.SelectMany(x => x.StockIds)).ToList();

            var stocks = await _session.LoadListFromMultipleIdsAsync<Stock>(stockIds);

            var purchaseList = new List<PurchaseListItem>();

            foreach (var item in purchases)
            {
                var purchaseItem = new PurchaseListItem
                {
                    SupplierName = suppliers.FirstOrDefault(c => c.Id == item.SupplierId)?.Name,
                    Id = item.Id,
                    SupplierId = item.SupplierId,
                    PurchaseDate = item.PurchaseDate,
                    PurchaseNumber = item.PurchaseNumber
                };

                var stockIdsForItem = item.PurchaseDetails.SelectMany(c => c.StockIds).ToList();
                var stocksForItem = stocks.Where(c => c.Id.In(stockIdsForItem)).ToList();

                purchaseItem.StockIn = new StockInfo(stocksForItem.Where(c => c.IsStockIn).Sum(c => c.Bags), stocksForItem.Where(c => c.IsStockIn).Sum(c => c.WeightKg));
                purchaseItem.StockOut = new StockInfo(stocksForItem.Where(c => !c.IsStockIn).Sum(c => c.Bags), stocksForItem.Where(c => c.IsStockIn).Sum(c => c.WeightKg));
                purchaseItem.StockBalance = new StockInfo(purchaseItem.StockIn.Bags + purchaseItem.StockOut.Bags, purchaseItem.StockIn.WeightKg + purchaseItem.StockOut.WeightKg);

                purchaseList.Add(purchaseItem);
            }

            return purchaseList;
        }
    }
}