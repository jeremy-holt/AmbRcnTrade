using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface ICounterService
    {
        Task<long> GetNextContractNumber(string companyId);
        Task<long> GetNextLotNumber(string companyId);
        Task<long> GetNextPurchaseNumber(string companyId);
    }

    public class CounterService : ICounterService
    {
        private const string CONTRACT_NUMBERS = "ContractNumbers";
        private const string LOT_NUMBERS = "LotNumbers";
        private const string PURCHASE_NUMBER = "PurchaseNumbers";

        private readonly IAsyncDocumentSession _session;

        public CounterService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<long> GetNextContractNumber(string companyId)
        {
            return await NextNumber(companyId, CONTRACT_NUMBERS);
        }

        public async Task<long> GetNextLotNumber(string companyId)
        {
            return await NextNumber(companyId, LOT_NUMBERS);
        }

        public async Task<long> GetNextPurchaseNumber(string companyId)
        {
            return await NextNumber(companyId, PURCHASE_NUMBER);
        }

        private async Task<long> NextNumber(string companyId, string key)
        {
            _session.CountersFor(companyId).Increment(key);
            await _session.SaveChangesAsync();
            return await GetCurrentCounter(companyId, key);
        }

        private async Task<long> GetCurrentCounter(string companyId, string key)
        {
            return await _session.CountersFor(companyId).GetAsync(key) ?? 0;
        }
    }
}