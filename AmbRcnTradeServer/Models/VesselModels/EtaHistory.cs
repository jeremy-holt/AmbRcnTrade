using System;

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