using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.Attachments
{
    public class AttachmentInfo
    {
        public string Name { get; set; }
        public string Route { get; set; }
        public long Size { get; set; }
        public string DisplayName { get; set; }
        public ImageType ImageType { get; set; }
    }
}