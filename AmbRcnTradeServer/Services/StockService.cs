using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Inspection = AmbRcnTradeServer.Models.InspectionModels.Inspection;

namespace AmbRcnTradeServer.Services
{
    public interface IStockService
    {
        Task<ServerResponse<Stock>> Save(Stock stock);
        Task<Stock> Load(string id);
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

            if (stock.StockOutDate != null)
            {
                stock.Bags = -stock.Bags;
                stock.WeightKg = -stock.WeightKg;
            }

            if (stock.Id.IsNullOrEmpty() && stock.IsStockIn)
            {
                stock.LotNo = await _counterService.GetNextLotNumber(stock.CompanyId);
            }

            await _session.StoreAsync(stock);
            return new ServerResponse<Stock>(stock, "Saved");
        }

        public async Task<Stock> Load(string id)
        {
            var stock = await _session.Include<Stock>(c => c.InspectionIds).LoadAsync<Stock>(id);
            var results = await _session.LoadAsync<Inspection>(stock.InspectionIds);

            var inspections = results.Values;

            stock.AnalysisResult = new Analysis
            {
                Count = CalculateAnalysisResult(inspections, c => c.Count),
                Kor = CalculateAnalysisResult(inspections, c => c.Kor),
                Moisture = CalculateAnalysisResult(inspections, c => c.Moisture),
                Rejects = CalculateAnalysisResult(inspections, c => c.Rejects),
                Sound = CalculateAnalysisResult(inspections, c => c.Sound),
                Spotted = CalculateAnalysisResult(inspections, c => c.Spotted)
            };

            return stock;
        }

        private static double CalculateAnalysisResult(IReadOnlyCollection<Inspection> inspections, Func<Analysis, double> field)
        {
            var bags = inspections.Sum(c => c.Bags);
            return inspections.Sum(c => c.Bags * field(c.AnalysisResult)) / bags;
        }
    }
}