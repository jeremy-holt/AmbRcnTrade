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
        public double Spotted { get; set; }
        public double Sound { get; set; }
        public double Rejects { get; set; }
        public double Kor { get; set; }
        public Approval Approved { get; set; }
        public double Bags { get; set; }
    }
}