using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.AttachmentModels
{
    public class PostDocumentRequest : BaseAttachmentRequest
    {
        public override string FileName { get; set; }
        public IFormFile File { get; set; }
    }
}