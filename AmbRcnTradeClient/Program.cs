using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Initializations;
using AmbRcnTradeServer;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ProgramInitialization.InitMain<Startup>(args);
        }
    }
}
