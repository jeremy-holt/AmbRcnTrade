using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class StockBalanceListItem
    {
        public long LotNo { get; set; }
        
        public double BagsIn { get; set; }
        public double BagsOut { get; set; }
        public double Balance { get; set; }
        public string LocationName { get; set; }
        public string LocationId { get; set; }
        public bool IsStockZero { get; set; }
        public List<AnalysisResult> AnalysisResults { get; set; } = new();
        public List<string> InspectionIds { get; set; } = new();
        public double Kor { get; set; }
        public double Moisture { get; set; }
        public double Count { get; set; }
        public double BalanceStockWeightKg { get; set; }
    }
}