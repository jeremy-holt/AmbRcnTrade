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
        public async Task<ActionResult<PaymentDto>> Create(string companyId)
        {
            return await Task.FromResult(new PaymentDto() {Payment = new Payment(){CompanyId = companyId}});
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<PaymentDto>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<PaymentDto>>> Save(PaymentDto payment)
        {
            return await _service.Save(payment);
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