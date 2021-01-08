using System.Collections.Generic;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.PaymentModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class PaymentsController: RavenController
    {
        private readonly IPaymentService _service;
        public PaymentsController(IAsyncDocumentSession session, IPaymentService service) : base(session)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Payment>> Create(string companyId)
        {
            return await Task.FromResult(new Payment() {CompanyId = companyId});
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Payment>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Payment>>> Save(Payment payment)
        {
            return await _service.Save(payment);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<PaymentDto>> LoadPaymentsPurchasesList(string companyId, string supplierId)
        {
            return await _service.LoadPaymentsPurchasesList(companyId, supplierId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<PaymentListItem>>> LoadList(string companyId, string supplierId)
        {
            return await _service.LoadList(companyId,supplierId);
        }

        [Authorize]
        [HttpDelete("[action]")]
        public async Task<ActionResult<ServerResponse>> DeletePayment(string id)
        {
            return await _service.DeletePayment(id);
        }
    }
}