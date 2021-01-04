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
    public class VesselController : RavenController
    {
        private readonly IVesselService _service;

        public VesselController(IAsyncDocumentSession session, IVesselService service) : base(session)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<VesselDto>>> Save(VesselDto vessel)
        {
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
            return await _service.LoadList(companyId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<NotLoadedContainer>>> GetNotLoadedContainers(string companyId)
        {
            return await _service.GetNotLoadedContainers(companyId);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> AddContainersToVessel(VesselContainersRequest request)
        {
            return await _service.AddContainerToVessel(request.VesselId, request.ContainerIds);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> RemoveContainersFromVessel(VesselContainersRequest request)
        {
            return await _service.RemoveContainersFromVessel(request.VesselId, request.ContainerIds);
        }
    }

    public class VesselContainersRequest
    {
        public IEnumerable<string> ContainerIds { get; set; }
        public string VesselId { get; set; }
    }
}