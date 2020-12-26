using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IPortsService
    {
        Task<ServerResponse<Port>> SavePort(Port port);
        Task<Port> LoadPort(string id);
        Task<List<Port>> LoadPortList(string companyId);
    }

    public class PortsService : IPortsService
    {
        private readonly IAsyncDocumentSession _session;

        public PortsService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<ServerResponse<Port>> SavePort(Port port)
        {
            await _session.StoreAsync(port);
            return new ServerResponse<Port>(port, $"Saved {port.Name}");
        }

        public async Task<Port> LoadPort(string id)
        {
            return await _session.LoadAsync<Port>(id);
        }

        public async Task<List<Port>> LoadPortList(string companyId)
        {
            return await Queryable.Where(_session.Query<Port>(), c => c.CompanyId == companyId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}