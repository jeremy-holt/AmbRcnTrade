using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        Task<ServerResponse> UnstuffContainer(string containerId, string stockId);
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

        public async Task<ServerResponse> UnstuffContainer(string containerId, string stockId)
        {
            var stock = await _session
                .Include<Stock>(c => c.StuffingRecords.Select(x => x.ContainerId))
                .LoadAsync<Stock>(stockId);

            var container = await _session.LoadAsync<Container>(containerId);

            var removableStatus = new[] {ContainerStatus.Cancelled, ContainerStatus.Empty, ContainerStatus.Stuffing};
            if (!container.Status.In(removableStatus))
                throw new InvalidOperationException("Cannot remove stock from a container that is no longer in the warehouse");

            foreach (var incomingStock in container.IncomingStocks)
            {
                incomingStock.Bags -= stock.Bags;
                incomingStock.WeightKg -= stock.WeightKg;
                incomingStock.StockIds.RemoveAll(c => c == stockId);
            }

            container.Bags -= stock.Bags;
            container.StuffingWeightKg -= stock.WeightKg;

            if (container.IncomingStocks.All(x => !x.StockIds.Any()))
            {
                container.IncomingStocks = new List<IncomingStock>();
            }
            

            return new ServerResponse("Removed stock from container");
        }
    }
}