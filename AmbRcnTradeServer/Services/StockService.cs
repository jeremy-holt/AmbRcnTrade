using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.StockModels;
using AmbRcnTradeServer.RavenIndexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IStockService
    {
        Task<ServerResponse<Stock>> Save(Stock stock);
        Task<Stock> Load(string id);
        Task<List<StockBalance>> LoadStockBalanceList(string companyId, string locationId);
        Task<List<StockListItem>> LoadStockList(string companyId, string locationId);
        Task<List<StockListItem>> LoadStockList(string companyId, List<string> stockIds);
        Task<ServerResponse> DeleteStock(string stockId);
    }

    public class StockService : IStockService
    {
        private readonly ICounterService _counterService;
        private readonly IInspectionService _inspectionService;
        private readonly IAsyncDocumentSession _session;

        public StockService(IAsyncDocumentSession session, ICounterService counterService, IInspectionService inspectionService)
        {
            _session = session;
            _counterService = counterService;
            _inspectionService = inspectionService;
        }

        public async Task<ServerResponse<Stock>> Save(Stock stock)
        {
            if (stock.StockInDate != null && stock.StockOutDate != null)
                throw new InvalidOperationException("A stock cannot have both a Stock In date and a Stock Out date");

            stock.AnalysisResult = await _inspectionService.GetAnalysisResult(stock.InspectionId);
            stock.IsStockIn = stock.StockInDate != null;

            // if (!stock.IsStockIn)
            // {
            //     stock.Bags = -stock.Bags;
            //     stock.WeightKg = -stock.WeightKg;
            // }

            if (stock.Id.IsNullOrEmpty() && stock.IsStockIn && stock.LotNo == 0)
            {
                stock.LotNo = await _counterService.GetNextLotNumber(stock.CompanyId);
            }

            await _session.StoreAsync(stock);
            return new ServerResponse<Stock>(stock, "Saved");
        }

        public async Task<Stock> Load(string id)
        {
            var stock = await _session.LoadAsync<Stock>(id);

            if (stock.InspectionId.IsNotNullOrEmpty())
            {
                stock.AnalysisResult = await _inspectionService.GetAnalysisResult(stock.InspectionId);
            }

            return stock;
        }

        public async Task<List<StockBalance>> LoadStockBalanceList(string companyId, string locationId)
        {
            var query = _session.Query<Stocks_ByBalances.Result, Stocks_ByBalances>()
                .OrderBy(c => c.LotNo)
                .Where(c => c.CompanyId == companyId);

            if (locationId.IsNotNullOrEmpty())
                query = query.Where(c => c.LocationId == locationId);

            var list = await query
                .ProjectInto<StockBalance>()
                .ToListAsync();

            foreach (var item in list)
            {
                item.Kor = item.BagsIn == 0 ? 0 : item.AnalysisResults.Average(c => c.Kor);
                item.Count = item.BagsIn == 0 ? 0 : item.AnalysisResults.Average(c => c.Count);
                item.Moisture = item.BagsIn == 0 ? 0 : item.AnalysisResults.Average(c => c.Moisture);

                item.Balance = item.BagsIn - item.BagsOut;
                item.BalanceWeightKg = item.WeightKgIn - item.WeightKgOut;
                item.IsStockZero = item.Balance < 1 && item.Balance > -1;
            }

            return list;
        }

        public Task<List<StockListItem>> LoadStockList(string companyId, string locationId)
        {
            return LoadStockList(companyId, locationId, null);
        }

        public async Task<List<StockListItem>> LoadStockList(string companyId, List<string> stockIds)
        {
            return await LoadStockList(companyId, null, stockIds);
        }

        public async Task<ServerResponse> DeleteStock(string stockId)
        {
            var stock = await _session
                .Include<Stock>(c => c.InspectionId)
                .LoadAsync<Stock>(stockId);


            if (stock.IsStockIn && stock.StuffingRecords.Any() || !stock.IsStockIn)
                throw new InvalidOperationException("This stock has already been stuffed into a container. It cannot be deleted");

            var inspection = await _session.LoadAsync<Inspection>(stock.InspectionId);
            inspection.StockReferences.RemoveAll(c => c.StockId == stockId);

            _session.Delete(stock);

            return new ServerResponse("Deleted stock");
        }

        private async Task<List<StockListItem>> LoadStockList(string companyId, string locationId, List<string> stockIds)
        {
            var query = _session.Query<StockListItem, Stocks_ById>()
                .Where(c => c.CompanyId == companyId);

            if (locationId.IsNotNullOrEmpty())
                query = query.Where(c => c.LocationId == locationId);

            if (stockIds != null && stockIds.Any())
                query = query.Where(c => c.StockId.In(stockIds));

            var stocks = await query
                .OrderBy(c => c.LotNo)
                .ThenBy(c => c.StockDate)
                .ThenBy(c => c.BagsOut)
                .ProjectInto<StockListItem>()
                .ToListAsync();

            return stocks;
        }
    }
}