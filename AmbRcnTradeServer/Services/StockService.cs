using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
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
        Task<List<StockBalanceListItem>> LoadStockBalanceList(string companyId, long? lotNo, string locationId);
        Task<List<StockListItem>> LoadStockList(string companyId, long? lotNo, string locationId);
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

            if (!stock.IsStockIn)
            {
                stock.Bags = -stock.Bags;
                stock.WeightKg = -stock.WeightKg;
            }

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

        public async Task<List<StockBalanceListItem>> LoadStockBalanceList(string companyId, long? lotNo, string locationId)
        {
            var query = _session.Query<Stocks_ByBalances.Result, Stocks_ByBalances>()
                .OrderBy(c => c.LotNo)
                .Where(c => c.CompanyId == companyId);

            if (lotNo != null)
                query = query.Where(c => c.LotNo == lotNo);

            if (locationId.IsNotNullOrEmpty())
                query = query.Where(c => c.LocationId == locationId);

            var list = await query
                .ProjectInto<StockBalanceListItem>()
                .ToListAsync();

            return list;
        }

        public async Task<List<StockListItem>> LoadStockList(string companyId, long? lotNo, string locationId)
        {
            var query = _session.Query<StockListItem, Stocks_ById>()
                .Where(c => c.CompanyId == companyId);

            if (lotNo != null)
                query = query.Where(c => c.LotNo == lotNo);

            if (locationId.IsNotNullOrEmpty())
                query = query.Where(c => c.LocationId == locationId);

            var stocks = await query
                .OrderBy(c => c.LotNo)
                .ThenBy(c => c.StockDate)
                .ProjectInto<StockListItem>()
                .ToListAsync();

            return stocks;
        }
    }
}