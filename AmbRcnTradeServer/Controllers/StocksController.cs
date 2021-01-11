using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.StockModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class StocksController : RavenController
    {
        private readonly IStockService _service;
        private readonly IAuditingService _auditingService;

        public StocksController(IAsyncDocumentSession session, IStockService service, IAuditingService auditingService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Stock>>> Save(Stock stock)
        {
            await _auditingService.Log(Request);
            return await _service.Save(stock);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Stock>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<StockListItem>>> LoadStockList(string companyId, long? lotNo, string locationId)
        {
            return await _service.LoadStockList(companyId, locationId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<StockBalance>>> LoadStockBalanceList(string companyId, long? lotNo, string locationId)
        {
            await _auditingService.Log(Request);
            return await _service.LoadStockBalanceList(companyId, locationId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Stock>> Create(string companyId)
        {
            return await Task.FromResult(new Stock
            {
                CompanyId = companyId, StockInDate = DateTime.Today
            });
        }

        [Authorize]
        [HttpDelete("[action]")]
        public async Task<ActionResult<ServerResponse>> Delete(string id)
        {
            await _auditingService.Log(Request);
            return await _service.DeleteStock(id);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> ZeroStock(ZeroStockRequest request)
        {
            return await _service.ZeroStock(request.CompanyId, request.LotNo);
        }
    }

    public class ZeroStockRequest
    {
        public string CompanyId { get; set; }
        public long LotNo { get; set; }
    }
}