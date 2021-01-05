﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.StockManagementModels
{
    public class MoveInspectionToStockRequest
    {
        public string InspectionId { get; set; }
        public double Bags { get; set; }
        public DateTime Date { get; set; }
        public string LocationId { get; set; }
        public long LotNo { get; set; }
    }
}