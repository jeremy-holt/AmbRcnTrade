using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class ContractController : RavenController
    {
        private readonly IContractService _contractService;

        public ContractController(IAsyncDocumentSession session, IContractService contractService) : base(session)
        {
            _contractService = contractService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Contract>>> Save(Contract contract)
        {
            return await _contractService.Save(contract);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Contract>> Load(string id)
        {
            return await _contractService.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<ContractListItem>>> LoadContainerList(ContractQueryParameters prms)
        {
            return await _contractService.LoadContainersList(prms);
        }
    }
}