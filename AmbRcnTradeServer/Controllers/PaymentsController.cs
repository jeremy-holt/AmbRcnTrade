using System;
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
        private readonly IAuditingService _auditingService;

        public PaymentsController(IAsyncDocumentSession session, IPaymentService service, IAuditingService auditingService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Payment>> Create(string companyId)
        {
            return await Task.FromResult(new Payment() {CompanyId = companyId, PaymentDate = DateTime.Today});
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
            await _auditingService.Log(Request);
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
            await _auditingService.Log(Request);
            return await _service.DeletePayment(id);
        }
    }
}