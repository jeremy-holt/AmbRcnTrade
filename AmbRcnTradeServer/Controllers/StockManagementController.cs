using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmberwoodCore.Services;
using AmbRcnTradeServer.Models.ContainerModels;
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
        private readonly IAuditingService _auditingService;

        public StockManagementController(IAsyncDocumentSession session, IStockManagementService service, IAuditingService auditingService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<MovedInspectionResult>>> MoveInspectionToStock(MoveInspectionToStockRequest request)
        {
            await _auditingService.Log(Request, request.InspectionId);
            return await _service.MoveInspectionToStock(request.InspectionId, request.Bags, request.WeightKg, request.Date, request.LotNo, request.LocationId, request.Origin, request.Fiche, request.Price);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> RemoveInspectionFromStock(RemoveInspectionFromStockRequest request)
        {
            await _auditingService.Log(Request, request.InspectionId, request.StockId);
            return await _service.RemoveInspectionFromStock(request.InspectionId, request.StockId);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<OutgoingStock>>> StuffContainer(StuffingRequest request)
        {
            await _auditingService.Log(Request, request.ContainerId);
            return await _service.StuffContainer(request.ContainerId, request.Status, request.StockBalance, request.Bags, request.WeightKg, request.StuffingDate);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<StockListItem>>> GetNonCommittedStocks(string companyId, string supplierId)
        {
            return await _service.GetNonCommittedStocks(companyId, supplierId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<AvailableContainer>>> GetAvailableContainers(string companyId)
        {
            return await _service.GetAvailableContainers(companyId);
        }
    }
}