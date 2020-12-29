using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Attributes;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Authorization;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class AuditingController : RavenController
    {
        private readonly IAuditingService _auditingService;

        public AuditingController(IAsyncDocumentSession session, IAuditingService auditingService) : base(session)
        {
            _auditingService = auditingService;
        }

        [AuthorizeRoles(RoleNames.ADMIN)]
        [HttpGet("[action]")]
        public async Task<List<AuditLog>> LoadList()
        {
            return await _auditingService.LoadList();
        }

        [AuthorizeRoles(RoleNames.ADMIN)]
        [HttpPost("[action]")]
        public async Task<ServerResponse> ClearLogs(ClearAuditLogRequest request)
        {
            return await _auditingService.ClearLogs(request.Days);
        }
    }

    public class ClearAuditLogRequest
    {
        public int Days { get; set; }
    }
}