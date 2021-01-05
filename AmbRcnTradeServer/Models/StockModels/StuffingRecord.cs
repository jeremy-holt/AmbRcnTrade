using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class StuffingRecord
    {
        public string ContainerId { get; set; }
        public string ContainerNumber { get; set; }
        public DateTime StuffingDate { get; set; }
    }
}