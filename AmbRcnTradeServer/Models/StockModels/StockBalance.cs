using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class StockBalance
    {
        public long LotNo { get; set; }

        public double BagsIn { get; set; }
        public double BagsOut { get; set; }
        public double Balance { get; set; }
        public string LocationName { get; set; }
        public string LocationId { get; set; }
        public List<AnalysisResult> AnalysisResults { get; set; } = new();
        public double Kor { get; set; }
        public double Moisture { get; set; }
        public double Count { get; set; }
        public double WeightKgIn { get; set; }
        public double WeightKgOut { get; set; }
        public double BalanceWeightKg { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public double AvgBagWeightKg { get; set; }
        public bool? ZeroedStock { get; set; }
    }
}