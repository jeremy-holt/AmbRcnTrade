using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Constants
{
    public enum ImageType
    {
        Image = 1,
        Pdf = 2,
        Other = 3
    }
}