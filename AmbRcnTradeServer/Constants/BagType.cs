using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Constants
{
    public enum BagType
    {
        Export = 1,
        Bush = 2
    }
}