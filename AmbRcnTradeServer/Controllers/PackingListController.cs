using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.PackingListModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class PackingListController : RavenController
    {
        private readonly IPackingListService _packingListService;

        public PackingListController(IAsyncDocumentSession session, IPackingListService packingListService) : base(session)
        {
            _packingListService = packingListService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<PackingList>>> Save(PackingList packingList)
        {
            return await _packingListService.Save(packingList);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<PackingList>> Load(string id)
        {
            return await _packingListService.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<PackingList>>> LoadList(string companyId)
        {
            return await _packingListService.LoadList(companyId);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<PackingList>>> RemoveContainerFromPackingList(RemoveContainerFromPackingListRequest request)
        {
            return await _packingListService.RemoveContainerFromPackingList(request.ContainerId, request.PackingListId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<PackingList>> Create(string companyId)
        {
            return await Task.FromResult(new PackingList {CompanyId = companyId, Date = DateTime.Today});
        }

        [Authorize]
        [HttpDelete("[action]")]
        public async Task<ActionResult<ServerResponse>> Delete(string id)
        {
            return await _packingListService.Delete(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Container>>> GetNonAllocatedContainers(string companyId)
        {
            return await _packingListService.GetNonAllocatedContainers(companyId);
        }
    }

    public class RemoveContainerFromPackingListRequest
    {
        public string ContainerId { get; set; }
        public string PackingListId { get; set; }
    }
}