using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.VesselModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.DraftBillLadingModels
{
    public class DraftBillLadingDataResponse
    {
        public Vessel Vessel { get; set; }
        public BillLadingDto BillLadingDto { get; set; }
        public Port Ports { get; set; }
        public List<Customer> Customers { get; set; }
    }
}