using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.PurchaseModels
{
    public class Purchase : IEntityCompany
    {
        public List<PurchaseDetail> PurchaseDetails { get; set; } = new();
        public long PurchaseNumber { get; set; }
        public string SupplierId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
    }
}