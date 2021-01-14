using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmberwoodCore.Services;
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
        private readonly IAuditingService _auditingService;
        private readonly IVesselService _service;

        public VesselController(IAsyncDocumentSession session, IVesselService service, IAuditingService auditingService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<VesselDto>>> Save(VesselDto vessel)
        {
            await _auditingService.Log(Request, vessel.Id);
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
        [HttpGet("[action]")]
        public async Task<ActionResult<VesselDto>> Create(string companyId)
        {
            return await Task.FromResult(new VesselDto {CompanyId = companyId});
        }

        [Authorize]
        [HttpDelete("[action]")]
        public async Task<ActionResult<ServerResponse>> DeleteVessel(string id)
        {
            await _auditingService.Log(Request, id);
            return await _service.DeleteVessel(id);
        }
    }

    public class VesselContainersRequest
    {
        public IEnumerable<string> BillLadingIds { get; set; }
        public string VesselId { get; set; }
    }
}