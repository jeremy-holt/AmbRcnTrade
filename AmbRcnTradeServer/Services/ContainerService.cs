using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
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

            return await query.OrderBy(c => c.StuffingDate).ToListAsync();
        }
    }
}