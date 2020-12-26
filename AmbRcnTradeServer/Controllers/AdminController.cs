using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Attributes;
using AmberwoodCore.Controllers;
using AmberwoodCore.Models;
using AmberwoodCore.Responses;
using AmberwoodCore.Services;
using AmbRcnTradeServer.Authorization;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class AdminController : AdminBaseController
    {
        private readonly IAdminService _adminService;
        private readonly IAppUserService _appUserService;

        public AdminController(IAsyncDocumentSession session, IAdminService adminService, IAppUserService appUserService) : base(session, adminService)
        {
            _adminService = adminService;
            _appUserService = appUserService;
        }

        [AuthorizeRoles(Roles = RoleNames.ADMIN)]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<ListItem>>> GetRoleNames()
        {
            return await _adminService.GetKeyNamesFromRolesClass(new RoleNames());
        }

        [AuthorizeRoles(Roles = RoleNames.ADMIN)]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<AppUser>>> CreateAppUser(AppUserDto appUserDto)
        {
            var password = appUserDto.Password;
            var response = await _adminService.CreateUser(appUserDto, "en");
            response.Dto.Password = password;
            await _appUserService.SaveAppUserPassword(response.Dto);
            var appUser = new AppUser
            {
                Id = response.Dto.Id,
                Approved = response.Dto.Approved,
                Name = response.Dto.Name,
                FirstName = response.Dto.FirstName,
                LastName = response.Dto.LastName,
                Email = response.Dto.Email,
                PasswordHash = response.Dto.PasswordHash,
                Role = response.Dto.Role
            };
            
            return new ServerResponse<AppUser>(appUser, "Saved");
        }
    }
}