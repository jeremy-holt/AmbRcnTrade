using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.PurchaseModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IBagDeliveryService
    {
        Task<ServerResponse<BagDelivery>> Save(BagDelivery bagDelivery);
        Task<BagDelivery> Load(string id);
        Task<List<BagDeliveryListItem>> LoadList(string companyId, string supplierId);
    }

    public class BagDeliveryService : IBagDeliveryService
    {
        private readonly IAsyncDocumentSession _session;
        public BagDeliveryService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<ServerResponse<BagDelivery>> Save(BagDelivery bagDelivery)
        {
            await _session.StoreAsync(bagDelivery);
            return new ServerResponse<BagDelivery>(bagDelivery, "Saved");
        }

        public async Task<BagDelivery> Load(string id)
        {
            return await _session.LoadAsync<BagDelivery>(id);
        }

        public async Task<List<BagDeliveryListItem>> LoadList(string companyId, string supplierId)
        {
            var query = await _session.Query<BagDelivery>()
                .Where(c => c.CompanyId == companyId && c.SupplierId == supplierId)
                .ProjectInto<BagDeliveryListItem>()
                .OrderBy(c => c.DeliveryDate)
                .ToListAsync();

            return query;
        }
    }
}