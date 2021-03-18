using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmberwoodCore.Services;
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
        private readonly IAuditingService _auditingService;
        private readonly IInspectionExportService _inspectionExportService;
        private readonly IInspectionService _service;

        public InspectionController(IAsyncDocumentSession session, IInspectionService service, IAuditingService auditingService, IInspectionExportService inspectionExportService) : base(session)
        {
            _service = service;
            _auditingService = auditingService;
            _inspectionExportService = inspectionExportService;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse<Inspection>>> Save(Inspection inspection)
        {
            await _auditingService.Log(Request, inspection.Id);
            return await _service.Save(inspection);
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<Inspection>> Load(string id)
        {
            return await _service.Load(id);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<List<InspectionListItem>>> LoadList(InspectionQueryParams prms)
        {
            await _auditingService.Log(Request);
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
                },
                Analyses = new List<Analysis> {new()}
            });
        }

        [Authorize]
        [HttpDelete("[action]")]
        public async Task<ActionResult<ServerResponse>> DeleteInspection(string id)
        {
            await _auditingService.Log(Request, id);
            return await _service.DeleteInspection(id);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult> ExportInspections(List<InspectionListItem> inspections)
        {
            var workbook =  _inspectionExportService.GetWorkbook("InspectionExportTemplate.xlsx",inspections);
            var response = await _inspectionExportService.SaveWorkbook(workbook);
            return File(response.FileContents, response.ContentType, response.FileName);
        }
    }
}