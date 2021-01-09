using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.VesselModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class BillLadingController : RavenController
    {
        private readonly IBillLadingService _service;
        private readonly IAuditingService _auditingService;

        public BillLadingController(IAsyncDocumentSession session, IBillLadingService service, IAuditingService auditingService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<BillLadingDto>>> Save(BillLadingDto billLading)
        {
            await _auditingService.Log(Request);
            return await _service.Save(billLading);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<BillLadingDto>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<BillLadingListItem>>> LoadList(string companyId)
        {
            await _auditingService.Log(Request);
            return await _service.LoadList(companyId);
        }


        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> RemoveContainersFromBillLading(BillLadingContainersRequest request)
        {
            await _auditingService.Log(Request);
            return await _service.RemoveContainersFromBillLading(request.BillOfLadingId, request.ContainerIds);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Container>>> GetNotLoadedContainers(string companyId)
        {
            return await _service.GetNotLoadedContainers(companyId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<BillLadingDto>> Create(string companyId, string vesselId)
        {
            return await Task.FromResult(new BillLadingDto {CompanyId = companyId, VesselId = vesselId});
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> MoveBillLadingToVessel(MoveBillLadingRequest request)
        {
            return await _service.MoveBillLadingToVessel(request.BillLadingId, request.FromVesselId, request.ToVesselId);
        }
    }

    public class MoveBillLadingRequest
    {
        public string BillLadingId { get; set; }
        public string FromVesselId { get; set; }
        public string ToVesselId { get; set; }
    }

    public class BillLadingContainersRequest
    {
        public IEnumerable<string> ContainerIds { get; set; }
        public string BillOfLadingId { get; set; }
    }
}