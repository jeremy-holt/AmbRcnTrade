namespace AmbRcnTradeServer.Models.StockModels
{
    public class StockBalanceListItem
    {
        public long LotNo { get; set; }
        public StockInfo StockIn { get; set; }
        public StockInfo StockOut { get; set; }
        public StockInfo Balance => new(StockIn.Bags + StockOut.Bags, StockIn.WeightKg + StockOut.WeightKg);
        public bool IsStockZero => Balance.Bags > 1 || Balance.Bags < -1;

    }
}