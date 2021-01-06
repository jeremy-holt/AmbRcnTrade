using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.AttachmentModels;
using AmbRcnTradeServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Controllers
{
    public class BillLadingAttachmentsController : RavenController
    {
        private readonly IBillLadingAttachmentService _service;

        public BillLadingAttachmentsController(IAsyncDocumentSession session, IBillLadingAttachmentService service) : base(session)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> UploadImages()
        {
            return await _service.SaveImages(Request);
        }
        
        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> UploadDocuments()
        {
            return await _service.SaveDocuments(Request);
        }

        [Authorize]
        [HttpPost("[action]")]
        public async Task<ActionResult<ServerResponse>> DeleteAttachments(List<DeleteAttachmentRequest> requests)
        {
            return await _service.DeleteAttachments(requests);
        }


        [Authorize]
        [HttpGet("[action]")]
        public async Task<ActionResult<List<AttachmentInfo>>> GetDocumentRoutes(string contractId)
        {
            var controllerName = GetType().Name.Replace("Controller", "");
            return await _service.GetDocumentRoutes(Request, controllerName, contractId);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult> GetAttachment(string contractId, string fileName)
        {
            return await _service.GetAttachment(contractId, fileName);
        }
    }
}