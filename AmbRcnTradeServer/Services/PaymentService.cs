﻿using System.Collections.Generic;
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
        Task<List<PaymentListItem>> LoadList(string companyId, string supplierId);
        Task<ServerResponse> DeletePayment(string id);
        Task<PaymentDto> LoadPaymentsPurchasesList(string companyId, string supplierId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly ICounterService _counterService;
        private readonly IPurchaseService _purchaseService;
        private readonly IAsyncDocumentSession _session;

        public PaymentService(IAsyncDocumentSession session, IPurchaseService purchaseService, ICounterService counterService)
        {
            _session = session;
            _purchaseService = purchaseService;
            _counterService = counterService;
        }

        public async Task<ServerResponse<PaymentDto>> Save(PaymentDto paymentDto)
        {
            if (paymentDto.Payment.Id.IsNullOrEmpty() && paymentDto.Payment.PaymentNo == 0)
            {
                var next = await _counterService.GetNextPaymentNumber(paymentDto.Payment.CompanyId);
                paymentDto.Payment.PaymentNo = next;
            }

            await _session.StoreAsync(paymentDto.Payment);
            await _session.SaveChangesAsync();

            paymentDto.PaymentList = await LoadList(paymentDto.Payment.CompanyId, paymentDto.Payment.SupplierId);

            paymentDto.PurchaseList = await _purchaseService.LoadList(null, paymentDto.Payment.SupplierId);

            return new ServerResponse<PaymentDto>(paymentDto, "Saved");
        }

        public async Task<PaymentDto> Load(string id)
        {
            var payment = await _session
                .Include<Payment>(c => c.SupplierId)
                .Include(c => c.BeneficiaryId)
                .LoadAsync<Payment>(id);

            var paymentsList = await LoadList(payment.CompanyId, payment.SupplierId);

            var purchases = await _purchaseService.LoadList(null, payment.SupplierId);

            return new PaymentDto
            {
                Payment = payment,
                PurchaseList = purchases,
                PaymentList = paymentsList
            };
        }

        public async Task<List<PaymentListItem>> LoadList(string companyId, string supplierId)
        {
            var query = _session.Query<PaymentListItem, Payments_ById>()
                .Include(c => c.SupplierId)
                .Include(c => c.BeneficiaryId);

            if (companyId.IsNotNullOrEmpty())
                query = query.Where(c => c.CompanyId == companyId);

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

        public async Task<PaymentDto> LoadPaymentsPurchasesList(string companyId, string supplierId)
        {
            var paymentsList = await LoadList(companyId, supplierId);

            var purchases = await _purchaseService.LoadList(null, supplierId);

            return new PaymentDto
            {
                PurchaseList = purchases,
                PaymentList = paymentsList
            };
        }
    }
}