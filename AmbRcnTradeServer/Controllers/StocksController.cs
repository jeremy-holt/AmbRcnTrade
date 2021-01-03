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

        public StocksController(IAsyncDocumentSession session, IStockService service) : base(session)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Stock>>> Save(Stock stock)
        {
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
            return await _service.LoadStockList(companyId, lotNo, locationId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<StockBalance>>> LoadStockBalanceList(string companyId, long? lotNo, string locationId)
        {
            return await _service.LoadStockBalanceList(companyId, lotNo, locationId);
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
            return await _service.DeleteStock(id);
        }
    }
}