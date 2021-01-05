using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Interfaces
{
    public interface IAnalysisResult
    {
        public AnalysisResult AnalysisResult { get; set; }
    }
}