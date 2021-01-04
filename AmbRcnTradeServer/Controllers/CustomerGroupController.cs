using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class CustomerGroupController : RavenController
    {
        private readonly ICustomerGroupService _service;

        public CustomerGroupController(IAsyncDocumentSession session, ICustomerGroupService service) : base(session)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<CustomerGroup>>> Save(CustomerGroup customerGroup)
        {
            return await _service.Save(customerGroup);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<CustomerGroup>>> LoadList(string companyId)
        {
            return await _service.LoadList(companyId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<CustomerGroup>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<CustomerGroup>> Create(string companyId)
        {
            return await Task.FromResult(new CustomerGroup {CompanyId = companyId});
        }
    }
}