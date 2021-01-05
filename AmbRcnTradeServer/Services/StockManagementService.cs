using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Interfaces;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.StockManagementModels;
using AmbRcnTradeServer.Models.StockModels;
using AmbRcnTradeServer.RavenIndexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IStockManagementService
    {
        Task<ServerResponse<MovedInspectionResult>> MoveInspectionToStock(string inspectionId, double bags, DateTime date, long lotNo, string locationId);
        Task<ServerResponse> RemoveInspectionFromStock(string inspectionId, string stockId);
        Task<List<StockListItem>> GetNonCommittedStocks(string companyId, string supplierId);
        Task<List<AvailableContainer>> GetAvailableContainers(string companyId);
        Task<ServerResponse<OutgoingStock>> StuffContainer(string containerId, ContainerStatus status, StockBalance stockBalance, double bags, double weightKg,
            DateTime stuffingDate);
    }

    public class StockManagementService : IStockManagementService
    {
        private readonly IInspectionService _inspectionService;
        private readonly IAsyncDocumentSession _session;
        private readonly IStockService _stockService;

        public StockManagementService(IAsyncDocumentSession session, IStockService stockService, IInspectionService inspectionService)
        {
            _stockService = stockService;
            _session = session;
            _inspectionService = inspectionService;
        }

        public async Task<ServerResponse<MovedInspectionResult>> MoveInspectionToStock(string inspectionId, double bags, DateTime date, long lotNo, string locationId)
        {
            Debug.WriteLine(_session.Advanced.NumberOfRequests);

            var inspection = await _inspectionService.Load(inspectionId);

            var stock = new Stock();
            {
                stock.Bags = bags;
                stock.StockInDate = date;
                stock.SupplierId = inspection.SupplierId;
                stock.CompanyId = inspection.CompanyId;
                stock.InspectionId = inspectionId;
                stock.LocationId = locationId;
                stock.LotNo = lotNo;
            }


            var stockResponse = await _stockService.Save(stock);

            if (inspection.StockReferences.FirstOrDefault(c => c.StockId == stockResponse.Id) == null)
            {
                inspection.StockReferences.Add(new StockReference(stockResponse.Id, bags, date, stock.LotNo));
                await _inspectionService.Save(inspection);
            }

            await _session.SaveChangesAsync();

            return new ServerResponse<MovedInspectionResult>(new MovedInspectionResult(stockResponse.Id, inspection), "Moved inspection to stock");
        }

        public async Task<ServerResponse> RemoveInspectionFromStock(string inspectionId, string stockId)
        {
            var inspection = await _session
                .Include<Inspection>(c => c.StockReferences.Select(x => x.StockId))
                .LoadAsync<Inspection>(inspectionId);

            var stocks = await _session.LoadListFromMultipleIdsAsync<Stock>(inspection.StockReferences.Select(c => c.StockId));

            inspection.StockReferences.RemoveAll(c => c.StockId == stockId);
            await _session.StoreAsync(inspection);

            foreach (var stock in stocks)
            {
                stock.InspectionId = null;
                await _session.StoreAsync(stock);
            }

            return new ServerResponse("Removed inspection from stock");
        }

        public async Task<List<StockListItem>> GetNonCommittedStocks(string companyId, string supplierId)
        {
            var stocks = await _session.Query<StockListItem, Stocks_ByPurchases>()
                .Where(c => c.CompanyId == companyId && c.IsStockIn && c.SupplierId == supplierId)
                .OrderBy(c => c.LotNo)
                .ProjectInto<StockListItem>()
                .ToListAsync();

            var stocksUsedInPurchasesIds = stocks.Where(c => c.PurchaseId.IsNotNullOrEmpty() && c.IsStockIn).Distinct().Select(c => c.StockId).ToList();
            var allStocksUsedInSystemIds = stocks.Where(c => c.IsStockIn).Distinct().Select(x => x.StockId).ToList();
            var unallocatedIds = allStocksUsedInSystemIds.Except(stocksUsedInPurchasesIds).ToList();
            var unallocatedStocks = stocks.Where(c => c.StockId.In(unallocatedIds)).ToList();

            return unallocatedStocks;
        }


        public async Task<List<AvailableContainer>> GetAvailableContainers(string companyId)
        {
            var containers = await _session.Query<Container>()
                .Where(c => c.CompanyId == companyId && (c.Status == ContainerStatus.Empty || c.Status == ContainerStatus.Stuffing))
                .ToListAsync();

            var list = new List<AvailableContainer>();
            foreach (var item in containers)
            {
                var availableContainer = new AvailableContainer
                {
                    Id = item.Id,
                    Status = item.Status,
                    BookingNumber = item.BookingNumber,
                    ContainerNumber = item.ContainerNumber,
                    Bags = item.Bags,
                    StockWeightKg = item.StuffingWeightKg,
                    IsOverweight = item.StuffingWeightKg > 26_000 || item.Bags > 325
                };
                list.Add(availableContainer);
            }

            return list.OrderBy(c=>Enum.GetName(typeof(ContainerStatus),c.Status)).ToList();
        }

        public async Task<ServerResponse<OutgoingStock>> StuffContainer(string containerId, ContainerStatus status, StockBalance stockBalance, double bags, double weightKg,
            DateTime stuffingDate)
        {
            var container = await _session.LoadAsync<Container>(containerId);
            var stocks = await _session.Query<Stock>().Where(c => c.LotNo == stockBalance.LotNo).ToListAsync();

            var incomingStock = new IncomingStock
            {
                LotNo = stockBalance.LotNo,
                Bags = bags,
                WeightKg = weightKg,
                StockIds = stocks.Select(c => new IncomingStockItem(c.Id,true)).ToList(),
                StuffingDate = stuffingDate
            };

            container.IncomingStocks.Add(incomingStock);

            container.Bags = container.IncomingStocks.Sum(x => x.Bags);
            container.StuffingWeightKg = container.IncomingStocks.Sum(x => x.WeightKg);
            container.Status = status;

            foreach (var stock in stocks)
            {
                stock.StuffingRecords.Add(new StuffingRecord {ContainerId = container.Id, ContainerNumber = container.ContainerNumber, StuffingDate = stuffingDate});
            }

            var stockOut = new Stock
            {
                LotNo = stockBalance.LotNo,
                Bags = bags,
                WeightKg = weightKg,
                StockOutDate = stuffingDate,
                AnalysisResult = stocks.AverageAnalysisResults(),
                LocationId = stockBalance.LocationId,
                IsStockIn = false,
                SupplierId = stockBalance.SupplierId,
                Origin = stocks.Select(x => x.Origin).ToAggregateString(),
                CompanyId = stocks.FirstOrDefault()?.CompanyId,
                StuffingRecords = new List<StuffingRecord>
                {
                    new()
                    {
                        ContainerId = containerId,
                        ContainerNumber = container.ContainerNumber,
                        StuffingDate = stuffingDate
                    }
                },

                StockInDate = null,
                Id = null,
                InspectionId = null
            };

            await _session.StoreAsync(stockOut);
            incomingStock.StockIds.Add(new IncomingStockItem(stockOut.Id,false));

            return new ServerResponse<OutgoingStock>(new OutgoingStock {StockId = stockOut.Id}, "Stuffed container");
        }
    }
}