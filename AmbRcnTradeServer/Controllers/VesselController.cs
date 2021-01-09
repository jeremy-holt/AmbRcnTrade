using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.VesselModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class VesselController : RavenController
    {
        private readonly IVesselService _service;
        private readonly IAuditingService _auditingService;

        public VesselController(IAsyncDocumentSession session, IVesselService service, IAuditingService auditingService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<VesselDto>>> Save(VesselDto vessel)
        {
            await _auditingService.Log(Request);
            return await _service.Save(vessel);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<VesselDto>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<VesselListItem>>> LoadList(string companyId)
        {
            await _auditingService.Log(Request);
            return await _service.LoadList(companyId);
        }


        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> RemoveBillsLadingFromVessel(VesselContainersRequest request)
        {
            await _auditingService.Log(Request);
            return await _service.RemoveBillsLadingFromVessel(request.VesselId, request.BillLadingIds);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<VesselDto>> Create(string companyId)
        {
            return await Task.FromResult(new VesselDto {CompanyId = companyId});
        }
    }

    public class VesselContainersRequest
    {
        public IEnumerable<string> BillLadingIds { get; set; }
        public string VesselId { get; set; }
    }
}