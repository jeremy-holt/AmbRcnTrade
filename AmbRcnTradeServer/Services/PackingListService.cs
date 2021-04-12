using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.PackingListModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IPackingListService
    {
        Task<ServerResponse<PackingList>> Save(PackingList packingList);
        Task<PackingList> Load(string id);
        Task<ServerResponse<PackingList>> RemoveContainerFromPackingList(string containerId, string packingListId);
        Task<List<PackingList>> LoadList(string companyId);
        Task<ServerResponse> Delete(string id);
        Task<List<Container>> GetNonAllocatedContainers(string companyId);
    }

    public class PackingListService : IPackingListService
    {
        private readonly IAsyncDocumentSession _session;

        public PackingListService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<ServerResponse<PackingList>> Save(PackingList packingList)
        {
            await _session.StoreAsync(packingList);

            var containers = await _session.Query<Container>()
                .ToListAsync();

            foreach (var container in containers)
            {
                if (packingList.ContainerIds.Contains(container.Id) && container.PackingListId.IsNullOrEmpty())
                    container.PackingListId = packingList.Id;

                if (container.PackingListId == packingList.Id && !packingList.ContainerIds.Contains(container.Id))
                    container.PackingListId = null;
            }

            return new ServerResponse<PackingList>(packingList, "Saved");
        }

        public async Task<PackingList> Load(string id)
        {
            var packingList = await _session.Include<PackingList>(c => c.ContainerIds).LoadAsync<PackingList>(id);
            var containers = await _session.LoadListFromMultipleIdsAsync<Container>(packingList.ContainerIds);

            packingList.Containers = containers;

            return packingList;
        }

        public async Task<ServerResponse<PackingList>> RemoveContainerFromPackingList(string containerId, string packingListId)
        {
            var packingList = await _session.Include<PackingList>(c => c.ContainerIds).LoadAsync<PackingList>(packingListId);
            var container = await _session.LoadAsync<Container>(containerId);

            packingList.ContainerIds.Remove(containerId);
            container.PackingListId = null;

            return new ServerResponse<PackingList>(packingList, "Removed container");
        }

        public async Task<List<PackingList>> LoadList(string companyId)
        {
            var packingLists = await _session.Query<PackingList>().Include(c => c.ContainerIds).OrderBy(c=>c.Id).ToListAsync();
            var containers = await _session.LoadListFromMultipleIdsAsync<Container>(packingLists.SelectMany(c => c.ContainerIds));

            foreach (var packingList in packingLists)
            {
                packingList.Containers = containers.Where(c => c.Id.In(packingList.ContainerIds)).ToList();
            }

            return packingLists;
        }

        public async Task<ServerResponse> Delete(string id)
        {
            var packingList = await _session.Include<PackingList>(c => c.ContainerIds).LoadAsync<PackingList>(id);
            var containers = await _session.LoadListFromMultipleIdsAsync<Container>(packingList.ContainerIds);

            foreach (var container in containers)
                container.PackingListId = null;

            _session.Delete(packingList);

            return new ServerResponse("Deleted packing list");
        }

        public async Task<List<Container>> GetNonAllocatedContainers(string companyId)
        {
            var query = await _session.Query<Container>()
                .Where(c => c.CompanyId == companyId && string.IsNullOrEmpty(c.PackingListId))
                .ToListAsync();

            return query;
        }
    }
}