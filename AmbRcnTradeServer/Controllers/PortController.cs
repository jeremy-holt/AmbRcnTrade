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
    public class PortController : RavenController
    {
        private readonly IPortsService _portsService;
        private readonly IAuditingService _auditingService;

        public PortController(IAsyncDocumentSession session, IPortsService portsService, IAuditingService auditingService) : base(session)
        {
            _portsService = portsService;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Port>>> Save(Port port)
        {
            await _auditingService.Log(Request);
            return await _portsService.SavePort(port);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Port>> Load(string id)
        {
            return await _portsService.LoadPort(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<Port>>> LoadPorts(string companyId)
        {
            return await _portsService.LoadPortList(companyId);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Port>> Create()
        {
            return await Task.FromResult(new Port());
        }
    }
}