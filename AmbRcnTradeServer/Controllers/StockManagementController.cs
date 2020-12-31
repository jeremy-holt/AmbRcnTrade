using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.StockManagementModels;
using AmbRcnTradeServer.Models.StockModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class StockManagementController : RavenController
    {
        private readonly IStockManagementService _service;

        public StockManagementController(IAsyncDocumentSession session, IStockManagementService service) : base(session)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<MovedInspectionResult>>> MoveInspectionToStock(MoveInspectionToStockRequest request)
        {
            return await _service.MoveInspectionToStock(request.InspectionId, request.Bags, request.Date, request.LotNo, request.LocationId);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> RemoveInspectionFromStock(RemoveInspectionFromStockRequest request)
        {
            return await _service.RemoveInspectionFromStock(request.InspectionId, request.StockId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Stock>>> GetNonCommittedStocks(string companyId)
        {
            return await _service.GetNonCommittedStocks(companyId);
        }
    }
}