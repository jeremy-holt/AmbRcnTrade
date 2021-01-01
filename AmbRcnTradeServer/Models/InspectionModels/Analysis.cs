using System;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class Analysis
    {
        public double Moisture { get; set; }
        public double Count { get; set; }
        public double SpottedGm { get; set; }
        public double SoundGm { get; set; }
        public double RejectsGm { get; set; }
        public double Kor { get; set; }
    }

    public class AnalysisResult
    {
        public double Moisture { get; set; }
        public double Count { get; set; }
        public double SpottedPct { get; set; }
        public double SoundPct { get; set; }
        public double RejectsPct { get; set; }
        public double Kor { get; set; }
        public Approval Approved { get; set; }
    }
        
}