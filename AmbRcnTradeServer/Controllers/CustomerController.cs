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
    public class CustomerController: RavenController
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
    }
}