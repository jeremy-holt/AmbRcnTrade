using System;
using System.Collections.Generic;
using AmberwoodCore.Interfaces;

namespace AmbRcnTradeServer.Models.PurchaseModels
{
    public class Purchase : IEntityCompany
    {
        public List<PurchaseDetail> PurchaseDetails { get; set; } = new();
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public long PurchaseNumber { get; set; }
        public string SupplierId { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}