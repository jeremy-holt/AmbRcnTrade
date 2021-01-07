using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public class CustomerController : RavenController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(IAsyncDocumentSession session, ICustomerService customerService) : base(session)
        {
            _customerService = customerService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Customer>>> Save(Customer customer)
        {
            return await _customerService.SaveCustomer(customer);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Customer>> Load(string id)
        {
            return await _customerService.LoadCustomer(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Customer>>> LoadAllCustomers(string companyId)
        {
            return await _customerService.LoadAllCustomers(companyId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<CustomerListItem>>> LoadCustomerListForAppUser(string companyId)
        {
            var appuserId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
            // var isAdmin = User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value.Contains("admin"));

            return await _customerService.LoadCustomerListForAppUser(companyId, appuserId, true);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Customer>> Create()
        {
            return await Task.FromResult(new Customer());
        }
    }
}