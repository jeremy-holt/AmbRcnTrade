using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.DictionaryModels;
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
        Task<List<StockBalanceListItem>> LoadStockBalanceList(string companyId, long? lotNo, string locationId);
        Task<List<StockListItem>> LoadStockList(string companyId, long? lotNo, string locationId);
    }

    public class StockService : IStockService
    {
        private readonly ICounterService _counterService;
        private readonly IAsyncDocumentSession _session;

        public StockService(IAsyncDocumentSession session, ICounterService counterService)
        {
            _session = session;
            _counterService = counterService;
        }

        public async Task<ServerResponse<Stock>> Save(Stock stock)
        {
            if (stock.StockInDate != null && stock.StockOutDate != null)
                throw new InvalidOperationException("A stock cannot have both a Stock In date and a Stock Out date");

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
            var stock = await _session.Include<Stock>(c => c.InspectionId).LoadAsync<Stock>(id);
            
            stock.Inspection = await _session.LoadAsync<Inspection>(stock.InspectionId);
            stock.AnalysisResult = stock.Inspection?.AnalysisResult;
            
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
            var query = _session.Query<Stock>()
                .Include(c => c.InspectionId)
                .Include(c => c.LocationId)
                .Include(c => c.SupplierId)
                .Where(c => c.CompanyId == companyId);

            if (lotNo != null)
                query = query.Where(c => c.LotNo == lotNo);

            if (locationId.IsNotNullOrEmpty())
                query = query.Where(c => c.LocationId == locationId);

            var stocks = await query.OrderBy(c => c.LotNo).ThenBy(c => c.StockInDate).ToListAsync();

            var locations = await _session.LoadListFromMultipleIdsAsync<Customer>(stocks.Select(c => c.LocationId));
            var suppliers = await _session.LoadListFromMultipleIdsAsync<Customer>(stocks.Select(x => x.SupplierId));
            var inspections = await _session.LoadListFromMultipleIdsAsync<Inspection>(stocks.Select(x => x.InspectionId));

            return stocks.Select(item => new StockListItem
                {
                    LotNo = item.LotNo,
                    LocationId = item.LocationId,
                    StockId = item.Id,
                    Date = item.StockInDate ?? item.StockOutDate ?? DateTime.MinValue,
                    IsStockIn = item.IsStockIn,
                    Origin = item.Origin,
                    StockIn = item.IsStockIn
                        ? new StockInfo(item.Bags, item.WeightKg)
                        : new StockInfo(),
                    StockOut = item.IsStockIn
                        ? new StockInfo()
                        : new StockInfo(item.Bags, item.WeightKg),
                    LocationName = locations.FirstOrDefault(c => c.Id == item.LocationId)?.Name,
                    SupplierId = item.SupplierId,
                    SupplierName = suppliers.FirstOrDefault(c => c.Id == item.SupplierId)?.Name,
                    InspectionId = item.InspectionId,
                    Inspection = inspections.FirstOrDefault(c => c.Id == item.InspectionId)
                })
                .ToList();
        }

        private static double GetAverageAnalysisResult(IEnumerable<Analysis> analyses, Func<Analysis, double> field, double bags)
        {
            if (bags == 0)
                return 0;
            
            return analyses.Where(c => c.Approved == Approval.Approved).Sum(x => x.Bags * field(x)) / bags;
        }

        private static double GetAverageAnalysisResultForStock(IEnumerable<Inspection> inspections, Func<Inspection, double> field)
        {
            var results = inspections.Where(c => c.AnalysisResult.Approved == Approval.Approved).ToList();

            return results.Any()
                ? results.Average(field)
                : 0;
        }
    }
}