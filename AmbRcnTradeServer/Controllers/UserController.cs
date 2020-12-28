using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Attributes;
using AmberwoodCore.Controllers;
using AmberwoodCore.Models;
using AmberwoodCore.Responses;
using AmberwoodCore.Services;
using AmbRcnTradeServer.Authorization;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class UserController : RavenController
    {
        private readonly IUserService _service;
        private readonly IAppUserService _appUserService;
        private readonly ICustomerService _customerService;

        public UserController(IAsyncDocumentSession session, IUserService service, IAppUserService appUserService, ICustomerService customerService) : base(session)
        {
            _service = service;
            _appUserService = appUserService;
            _customerService = customerService;
        }

        // [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<List<ListItem>>> GetCompaniesForUser(CompaniesForUserRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var list = await _service.GetCompaniesForUser(request.CompanyIds);
            return new ActionResult<List<ListItem>>(list);
        }
        
        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<AppUserInfo>>GetCustomersForAppUser(string appUserId)
        {
            return await _appUserService.GetCustomersForAppUser(appUserId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<CustomerUserListItem>>> ListCustomersAndUsers(string companyId)
        {
            return await _customerService.ListCustomersAndUsers(companyId);
        }

        [AuthorizeRoles(RoleNames.ADMIN)]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<List<AppUserPassword>>>> SaveAppUsersPasswords(List<AppUserPassword> list)
        {
            return await _appUserService.SaveAppUsersPasswords(list);
        }

        [AuthorizeRoles(RoleNames.ADMIN)]
        [HttpGet("[action]")]
        public async Task<List<AppUserPassword>> LoadAppUsersPasswords()
        {
            return await _appUserService.LoadAppUsersPasswords();
        }
    }

    public class CompaniesForUserRequest
    {
        public IEnumerable<string> CompanyIds { get; set; }
    }
}