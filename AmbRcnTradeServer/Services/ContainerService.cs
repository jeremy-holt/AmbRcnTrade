using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.StockModels;
using AmbRcnTradeServer.Models.VesselModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IContainerService
    {
        Task<ServerResponse<Container>> Save(Container container);
        Task<Container> Load(string id);
        Task<List<Container>> LoadList(string companyId, ContainerStatus? status);

        Task<ServerResponse> UnStuffContainer(string containerId);
        Task<ServerResponse> DeleteContainer(string id);
    }

    public class ContainerService : IContainerService
    {
        private readonly IAsyncDocumentSession _session;

        public ContainerService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<ServerResponse<Container>> Save(Container container)
        {
            container.Bags = container.IncomingStocks.Sum(c => c.Bags);
            container.StuffingWeightKg = container.IncomingStocks.Sum(c => c.WeightKg);
            container.NettWeightKg = container.WeighbridgeWeightKg - container.TareKg;

            await _session.StoreAsync(container);
            return new ServerResponse<Container>(container, "Saved");
        }

        public async Task<Container> Load(string id)
        {
            var container = await _session.Include<Container>(c => c.VesselId).LoadAsync<Container>(id);
            var vessel = await _session.LoadAsync<Vessel>(container.VesselId);
            container.VesselName = $"{vessel?.VesselName} {vessel?.VoyageNumber}";
            return container;
        }

        public async Task<List<Container>> LoadList(string companyId, ContainerStatus? status)
        {
            var query = _session.Query<Container>()
                .Include(c => c.VesselId)
                .Include(c => c.WarehouseId)
                .Where(c => c.CompanyId == companyId);

            if (status != null)
                query = query.Where(c => c.Status == status);

            var list = await query.ToListAsync();

            var vessels = await _session.LoadListFromMultipleIdsAsync<Vessel>(list.GetPropertyFromList(c => c.VesselId));
            var warehouses = await _session.LoadListFromMultipleIdsAsync<Customer>(list.GetPropertyFromList(c => c.WarehouseId));

            foreach (var container in list)
            {
                container.StuffingDate = container.IncomingStocks.FirstOrDefault()?.StuffingDate;
                var vessel = vessels.FirstOrDefault(c => c.Id == container.VesselId);
                container.VesselName = $"{vessel?.VesselName} {vessel?.VoyageNumber}";
                container.WarehouseName = warehouses.FirstOrDefault(c => c.Id == container.WarehouseId)?.Name;
            }

            return list
                .OrderBy(c => Enum.GetName(typeof(ContainerStatus), c.Status))
                .ThenBy(c => c.Id).ToList();
            // .ThenBy(c => c.IncomingStocks.OrderBy(incomingStock => incomingStock.StuffingDate).FirstOrDefault()?.StuffingDate).ToList();
        }

        public async Task<ServerResponse> UnStuffContainer(string containerId)
        {
            var container = await _session
                .Include<Container>(c => c.IncomingStocks.SelectMany(stock => stock.StockIds.Select(stockItem => stockItem.StockId)))
                .LoadAsync<Container>(containerId);

            var stocks = await _session.LoadListFromMultipleIdsAsync<Stock>(container.IncomingStocks.SelectMany(stock => stock.StockIds.Select(stockItem => stockItem.StockId)));
            var stockOuts = stocks.Where(c => !c.IsStockIn && containerId.In(c.StuffingRecords.GetPropertyFromList(x => x.ContainerId))).ToList();

            var removableStatus = new[] {ContainerStatus.Cancelled, ContainerStatus.Empty, ContainerStatus.Stuffing, ContainerStatus.StuffingComplete};

            if (!container.Status.In(removableStatus))
                throw new InvalidOperationException("Cannot remove stock from a container that is no longer in the warehouse");

            foreach (var stock in stocks)
            {
                stock.StuffingRecords.RemoveAll(c => c.ContainerId == containerId);
            }

            // var stockOuts = stocks.Where(c => !c.IsStockIn);
            foreach (var stockOut in stockOuts)
            {
                _session.Delete(stockOut);
            }

            container.IncomingStocks = new List<IncomingStock>();
            container.Bags = 0;
            container.StuffingWeightKg = 0;
            container.Status = ContainerStatus.Empty;

            return new ServerResponse("Unstuffed container");
        }

        public async Task<ServerResponse> DeleteContainer(string id)
        {
            var container = await _session.LoadAsync<Container>(id);

            if (container.IncomingStocks.Any())
                throw new InvalidOperationException("Cannot delete a container that has already been stuffed");

            var billOfLading = await _session.Query<BillLading>().Where(c => c.ContainerIds.Contains(id)).FirstOrDefaultAsync();

            billOfLading?.ContainerIds.Remove(id);

            _session.Delete(container);

            return new ServerResponse("Deleted container");
        }
    }
}