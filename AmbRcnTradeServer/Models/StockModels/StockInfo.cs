using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class StockInfo
    {
        public StockInfo(double bags, double weightKg)
        {
            Bags = bags;
            WeightKg = weightKg;
        }

        public StockInfo() { }
        public double Bags { get; set; }
        public double WeightKg { get; set; }

        public override string ToString()
        {
            return $"Bags: {Bags}, WeightKg: {WeightKg}";
        }
    }
}