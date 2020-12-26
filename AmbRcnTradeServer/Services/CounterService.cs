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
    }

    public class CounterService : ICounterService
    {
        private const string CONTRACT_NUMBERS = "ContractNumbers";
        private readonly IAsyncDocumentSession _session;

        public CounterService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<long> GetNextContractNumber(string companyId)
        {
            _session.CountersFor(companyId).Increment(CONTRACT_NUMBERS);
            await _session.SaveChangesAsync();
            return await GetCurrentCounter(companyId, CONTRACT_NUMBERS);
        }

        private async Task<long> GetCurrentCounter(string companyId, string key)
        {
            return await _session.CountersFor(companyId).GetAsync(key) ?? 0;
        }
    }
}