using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Constants
{
    public enum Currency
    {
        USD = 1,
        EUR = 2,
        CFA = 3,
        GBP = 4
    }
}