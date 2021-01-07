using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class ContainerController : RavenController
    {
        private readonly IContainerService _service;

        public ContainerController(IAsyncDocumentSession session, IContainerService service) : base(session)
        {
            _service = service;
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Container>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Container>>> LoadList(string companyId, ContainerStatus? status)
        {
            return await _service.LoadList(companyId, status);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ServerResponse<Container>> Save(Container container)
        {
            return await _service.Save(container);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Container>> Create(string companyId)
        {
            return await Task.FromResult(new Container {CompanyId = companyId});
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> UnstuffContainer(UnstuffContainerRequest request)
        {
            return await _service.UnStuffContainer(request.ContainerId);
        }

        [Authorize]
        [HttpDelete("[action]")]
        public async Task<ActionResult<ServerResponse>> DeleteContainer(string id)
        {
            return await _service.DeleteContainer(id);
        }

        public class UnstuffContainerRequest
        {
            public string ContainerId { get; set; }
        }
    }
}