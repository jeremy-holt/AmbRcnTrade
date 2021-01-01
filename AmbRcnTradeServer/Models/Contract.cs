using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models
{
    public class Contract : IEntityCompany
    {
        public string SellerId { get; set; }
        public string BuyerId { get; set; }
        public string BrokerId { get; set; }
        public List<Container> Containers { get; set; } = new();
        public List<PurchaseLot> PurchaseLots { get; set; } = new();
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
    }

    public class PurchaseLot
    {
        public Guid Id { get; set; }
        public double Price { get; set; }
        public double PurchaseKor { get; set; }
        public double PurchasedMoisture { get; set; }
        public Currency Currency { get; set; } = Currency.CFA;
        public List<Inspection> Inspections { get; set; } = new();
        public int Bags { get; set; }
        public List<Payment> Payments { get; set; } = new();
        public bool Approved { get; set; }
    }

    public class Payment
    {
        public DateTime? PaymentDate { get; set; }
        public string BeneficiaryId { get; set; }
        public double Price { get; set; }
        public Currency Currency { get; set; }
        public double ExchangeRate { get; set; }
    }
}