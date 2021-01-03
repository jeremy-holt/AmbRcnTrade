using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.StockModels;
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

            await _session.StoreAsync(container);
            return new ServerResponse<Container>(container, "Saved");
        }

        public async Task<Container> Load(string id)
        {
            return await _session.LoadAsync<Container>(id);
        }

        public async Task<List<Container>> LoadList(string companyId, ContainerStatus? status)
        {
            var query = Queryable.Where(_session.Query<Container>(), c => c.CompanyId == companyId);
            if (status != null)
                query = query.Where(c => c.Status == status);

            var list = await query.ToListAsync();
            return list.OrderBy(c => c.IncomingStocks.OrderBy(incomingStock => incomingStock.StuffingDate).FirstOrDefault()?.StuffingDate).ToList();
        }

        public async Task<ServerResponse> UnStuffContainer(string containerId)
        {
            var container = await _session
                .Include<Container>(c => c.IncomingStocks.SelectMany(stock => stock.StockIds.Select(stockItem => stockItem.StockId)))
                .LoadAsync<Container>(containerId);

            var stocks = await _session.LoadListFromMultipleIdsAsync<Stock>(container.IncomingStocks.SelectMany(stock => stock.StockIds.Select(stockItem => stockItem.StockId)));

            var removableStatus = new[] {ContainerStatus.Cancelled, ContainerStatus.Empty, ContainerStatus.Stuffing};
            if (!container.Status.In(removableStatus))
                throw new InvalidOperationException("Cannot remove stock from a container that is no longer in the warehouse");

            foreach (var stock in stocks)
            {
                stock.StuffingRecords.RemoveAll(c => c.ContainerId == containerId);
            }

            var stockOuts = stocks.Where(c => !c.IsStockIn);
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
    }
}