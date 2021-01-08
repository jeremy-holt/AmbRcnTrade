using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.PaymentModels;
using AmbRcnTradeServer.RavenIndexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IPaymentService
    {
        Task<ServerResponse<PaymentDto>> Save(PaymentDto paymentDto);
        Task<PaymentDto> Load(string id);
        Task<List<PaymentListItem>> LoadList(string companyId, string supplierId, string beneficiaryId);
        Task<ServerResponse> DeletePayment(string id);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IPurchaseService _purchaseService;
        private readonly IAsyncDocumentSession _session;

        public PaymentService(IAsyncDocumentSession session, IPurchaseService purchaseService)
        {
            _session = session;
            _purchaseService = purchaseService;
        }

        public async Task<ServerResponse<PaymentDto>> Save(PaymentDto paymentDto)
        {
            await _session.StoreAsync(paymentDto.Payment);
            await _session.SaveChangesAsync();

            paymentDto.PaymentList = await LoadList(paymentDto.Payment.CompanyId,paymentDto.Payment.SupplierId, null);

            paymentDto.PurchaseList = await _purchaseService.LoadList(null, paymentDto.Payment.SupplierId);

            return new ServerResponse<PaymentDto>(paymentDto, "Saved");
        }

        public async Task<PaymentDto> Load(string id)
        {
            var payment = await _session
                .Include<Payment>(c => c.SupplierId)
                .Include(c => c.BeneficiaryId)
                .LoadAsync<Payment>(id);

            var paymentsList = await LoadList(payment.CompanyId,payment.SupplierId,null);

            var purchases = await _purchaseService.LoadList(null, payment.SupplierId);

            return new PaymentDto
            {
                Payment = payment,
                PurchaseList = purchases,
                PaymentList = paymentsList
            };
        }

        public async Task<List<PaymentListItem>> LoadList(string companyId, string supplierId, string beneficiaryId)
        {
            var query = _session.Query<PaymentListItem, Payments_ById>()
                .Include(c => c.SupplierId)
                .Include(c => c.BeneficiaryId);

            if (companyId.IsNotNullOrEmpty())
                query = query.Where(c => c.CompanyId == companyId);

            if (beneficiaryId.IsNotNullOrEmpty())
                query = query.Where(c => c.BeneficiaryId == beneficiaryId);

            if (supplierId.IsNotNullOrEmpty())
                query = query.Where(c => c.SupplierId == supplierId);

            var payments = await query.ProjectInto<PaymentListItem>()
                .ToListAsync();

            var customers = await _session.LoadListFromMultipleIdsAsync<Customer>(payments.GetPropertyFromList(c => c.SupplierId)
                .Concat(payments.GetPropertyFromList(c => c.BeneficiaryId)));

            foreach (var payment in payments)
            {
                payment.SupplierName = customers.FirstOrDefault(c => c.Id == payment.SupplierId)?.Name;
                payment.BeneficiaryName = customers.FirstOrDefault(c => c.Id == payment.BeneficiaryId)?.Name;
            }

            return payments;
        }

        public async Task<ServerResponse> DeletePayment(string id)
        {
            _session.Delete(id);
            return await Task.FromResult(new ServerResponse("Deleted payment"));
        }
    }
}