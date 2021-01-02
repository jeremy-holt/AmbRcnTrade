using System;

namespace AmbRcnTradeServer.Models.StockModels
{
    public class StuffingRecord
    {
        public string ContainerId { get; set; }
        public string ContainerNumber { get; set; }
        public DateTime StuffingDate { get; set; }
    }
}