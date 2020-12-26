using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Constants
{
    public enum ContainerStatus
    {
        Open = 1,
        Shipped = 2,
        Cancelled = 3,
        ShippedWithoutDocuments = 4
    }
}