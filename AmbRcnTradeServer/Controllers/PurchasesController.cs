using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.PurchaseModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class PurchasesController : RavenController
    {
        private readonly IPurchaseService _service;
        private readonly IAuditingService _auditingService;

        public PurchasesController(IAsyncDocumentSession session, IPurchaseService service, IAuditingService auditingService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Purchase>>> Save(Purchase purchase)
        {
            await _auditingService.Log(Request);
            return await _service.Save(purchase);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Purchase>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<PurchaseListItem>>> LoadList(string companyId, string supplierId)
        {
            await _auditingService.Log(Request);
            return await _service.LoadList(companyId, supplierId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Purchase>> Create(string companyId)
        {
            return await Task.FromResult(new Purchase {CompanyId = companyId, PurchaseDate = DateTime.Today});
        }
    }
}