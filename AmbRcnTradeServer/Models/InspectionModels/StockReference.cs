using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class StockReference
    {
        public StockReference(string stockId, double bags, double weightKg, DateTime date, long lotNo)
        {
            StockId = stockId;
            Bags = bags;
            WeightKg = weightKg;
            Date = date;
            LotNo = lotNo;
        }

        public string StockId { get; set; }
        public double Bags { get; set; }

        public DateTime Date { get; set; }
        public long LotNo { get; set; }
        public double WeightKg { get; set; }
    }
}