using System.Threading.Tasks;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.Payments;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IPaymentService
    {
        Task<ServerResponse<Payment>> Save(Payment payment);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IAsyncDocumentSession _session;
        public PaymentService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<ServerResponse<Payment>> Save(Payment payment)
        {
            await _session.StoreAsync(payment);
            return new ServerResponse<Payment>(payment, "Saved");
        }
    }
}