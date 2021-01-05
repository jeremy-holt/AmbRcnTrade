using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Interfaces
{
    public static class ObjectExtensions
    {
        public static AnalysisResult AverageAnalysisResults(this IEnumerable<IAnalysisResult> list)
        {
            
            var analyses = list.ToList();
            
            return new AnalysisResult
            {
                Approved = Approval.Approved,
                Count = analyses.Where(c => c.AnalysisResult.Approved == Approval.Approved).Average(x => x.AnalysisResult.Count),
                Kor = analyses.Where(c => c.AnalysisResult.Approved == Approval.Approved).Average(x => x.AnalysisResult.Kor),
                Moisture = analyses.Where(c => c.AnalysisResult.Approved == Approval.Approved).Average(x => x.AnalysisResult.Moisture),
                SpottedPct = analyses.Where(c => c.AnalysisResult.Approved == Approval.Approved).Average(x => x.AnalysisResult.SpottedPct),
                SoundPct = analyses.Where(c => c.AnalysisResult.Approved == Approval.Approved).Average(x => x.AnalysisResult.SoundPct),
                RejectsPct = analyses.Where(c => c.AnalysisResult.Approved == Approval.Approved).Average(x => x.AnalysisResult.RejectsPct)
            };
        }
    }
}