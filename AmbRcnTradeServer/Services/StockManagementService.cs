using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.StockManagementModels;
using AmbRcnTradeServer.Models.StockModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IStockManagementService
    {
        Task<ServerResponse<MovedInspectionResult>> MoveInspectionToStock(string inspectionId, double bags, DateTime date, long lotNo, string locationId);
        Task<ServerResponse> RemoveInspectionFromStock(string inspectionId, string stockId);
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

            var stock = new Stock(); {
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
    }
}