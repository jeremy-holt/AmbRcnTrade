using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.AttachmentModels
{
    public class PostAttachmentRequest : BaseAttachmentRequest
    {
        public override string FileName
        {
            get => File?.Name;
            set => throw new InvalidOperationException("Cannot set FileName");
        }

        public IFormFile File { get; set; }
    }

    public abstract class BaseAttachmentRequest
    {
        public string ContractId { get; init; }
        public abstract string FileName { get; set; }
    }
}