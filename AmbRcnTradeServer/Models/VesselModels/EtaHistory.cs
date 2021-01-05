using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.VesselModels
{
    public class EtaHistory
    {
        public EtaHistory(string vesselName, DateTime? eta)
        {
            VesselName = vesselName;
            Eta = eta;
            DateUpdated = DateTime.Now;
        }

        public EtaHistory() { }
        public DateTime DateUpdated { get; set; }
        public string VesselName { get; set; }
        public DateTime? Eta { get; set; }
        public string Notes { get; set; }
    }
}