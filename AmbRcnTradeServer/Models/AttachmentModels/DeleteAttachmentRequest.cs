using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.AttachmentModels
{
    public class DeleteAttachmentRequest : BaseAttachmentRequest
    {
        public override string FileName { get; set; }
    }
}