using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Inspections_ByAnalysisResult : AbstractIndexCreationTask<Inspection, Inspections_ByAnalysisResult.Result>
    {
        public Inspections_ByAnalysisResult()
        {
            Map = inspections => from inspection in inspections
                from analysis in inspection.Analyses
                select new
                {
                    InspectionId = inspection.Id,
                    inspection.AnalysisResult.Approved,
                    analysis.Count,
                    Kor = ((analysis.SpottedGm * 0.5) + analysis.SoundGm) * 0.1763696,
                    analysis.Moisture,
                    analysis.SoundGm,
                    analysis.RejectsGm,
                    analysis.SpottedGm,
                    TotalRejects = analysis.RejectsGm + analysis.SpottedGm + analysis.SoundGm,
                    SoundPct = 0,
                    RejectsPct = 0,
                    SpottedPct = 0
                };

            Reduce = results => from c in results
                group c by new {c.InspectionId, c.Approved}
                into grp
                let totalRejects = grp.Sum(c => c.TotalRejects)
                select new
                {
                    grp.Key.InspectionId,
                    grp.Key.Approved,
                    Count = grp.Average(c => c.Count),
                    Kor = grp.Average(c => c.Kor),
                    Moisture = grp.Average(c => c.Moisture),
                    SoundGm = grp.Average(c => c.SoundGm),
                    RejectsGm = grp.Average(c => c.RejectsGm),
                    SpottedGm = grp.Average(c => c.SpottedGm),
                    TotalRejects = totalRejects,
                    SoundPct = grp.Sum(c => c.SoundGm) / totalRejects,
                    RejectsPct = grp.Sum(c => c.RejectsGm) / totalRejects,
                    SpottedPct = grp.Sum(c => c.SpottedGm) / totalRejects
                };

            Index(x => x.InspectionId, FieldIndexing.Default);
            Index(x => x.Approved, FieldIndexing.Default);
            Store(x => x.InspectionId, FieldStorage.Yes);
            Store(x => x.Count, FieldStorage.Yes);
            Store(x => x.Moisture, FieldStorage.Yes);
            Store(x => x.SoundPct, FieldStorage.Yes);
            Store(x => x.RejectsPct, FieldStorage.Yes);
            Store(x => x.SpottedPct, FieldStorage.Yes);
        }

        public class Result
        {
            public string InspectionId { get; set; }
            public double Count { get; set; }
            public double Kor { get; set; }
            public double Moisture { get; set; }
            public double SoundPct { get; set; }
            public double RejectsPct { get; set; }
            public double SpottedPct { get; set; }
            public double SoundGm { get; set; }
            public double RejectsGm { get; set; }
            public double SpottedGm { get; set; }
            public double TotalRejects { get; set; }
            public Approval Approved { get; set; }
        }
    }
}