using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class InspectionController : RavenController
    {
        private readonly IInspectionService _service;
        private readonly IAuditingService _auditingService;

        public InspectionController(IAsyncDocumentSession session, IInspectionService service, IAuditingService auditingService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Inspection>>> Save(Inspection inspection)
        {
            await _auditingService.Log(Request);
            return await _service.Save(inspection);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Inspection>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<InspectionListItem>>> LoadList(string companyId, Approval? approval)
        {
            await _auditingService.Log(Request);
            var prms = new InspectionQueryParams {CompanyId = companyId, Approved = approval};
            return await _service.LoadList(prms);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Inspection>> Create()
        {
            return await Task.FromResult(new Inspection
            {
                InspectionDate = DateTime.Today,
                AnalysisResult = new AnalysisResult
                {
                    Approved = Approval.Rejected
                }
            });
        }

        [Authorize]
        [HttpDelete("[action]")]
        public async Task<ActionResult<ServerResponse>> DeleteInspection(string id)
        {
            return await _service.DeleteInspection(id);
        }
    }
}