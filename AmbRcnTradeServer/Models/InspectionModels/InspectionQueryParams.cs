﻿using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.InspectionModels
{
    public class InspectionQueryParams
    {
        public string CompanyId { get; set; }
        public Approval? Approved { get; set; }
        public string WarehouseId { get; set; }
        public string SupplierId { get; set; }
        public string BuyerId { get; set; }
    }
}