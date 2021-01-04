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
    public interface ICustomerGroupService
    {
        Task<ServerResponse<CustomerGroup>> Save(CustomerGroup customerGroup);
        Task<CustomerGroup> Load(string id);
        Task<List<CustomerGroup>> LoadList(string companyId);
    }

    public class CustomerGroupService : ICustomerGroupService
    {
        private readonly IAsyncDocumentSession _session;

        public CustomerGroupService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<ServerResponse<CustomerGroup>> Save(CustomerGroup customerGroup)
        {
            await _session.StoreAsync(customerGroup);
            return new ServerResponse<CustomerGroup>(customerGroup, "Saved");
        }

        public async Task<CustomerGroup> Load(string id)
        {
            return await _session.LoadAsync<CustomerGroup>(id);
        }

        public async Task<List<CustomerGroup>> LoadList(string companyId)
        {
            return await Queryable.Where(_session.Query<CustomerGroup>(), c => c.CompanyId == companyId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}