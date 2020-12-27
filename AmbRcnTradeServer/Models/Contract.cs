using System;
using System.Collections.Generic;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models
{
    public class Contract: IEntityCompany
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public string SellerId { get; set; }
        public string BuyerId { get; set; }
        public string BrokerId { get; set; }
        public List<Container> Containers { get; set; } = new List<Container>();
        public List<PurchaseLot> PurchaseLots { get; set; } = new List<PurchaseLot>();
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
    }

    public class Container
    {
        public Guid Id { get; set; }
        public string ContainerNumber { get; set; }
        public string SealNumber { get; set; }
        public List<PurchaseLot> PurchaseLots { get; set; } = new List<PurchaseLot>();
        public List<string> Documents { get; set; } = new List<string>();
        public string ForwardingAgent { get; set; }
        public string VesselName { get; set; }
        public DateTime? VesselEta { get; set; }
        public string BlNumber { get; set; }
        public DateTime? BlDate { get; set; }
        public ContainerStatus Status { get; set; }
    }
    
    public class PurchaseLot
    {
        public Guid Id { get; set; }
        public double Price { get; set; }
        public double PurchaseKor { get; set; }
        public double PurchasedMoisture { get; set; }
        public Currency Currency { get; set; } = Currency.CFA;
        public List<Inspection> Inspections { get; set; } = new List<Inspection>();
        public int Bags { get; set; }
        public List<Payment> Payments { get; set; } = new List<Payment>();
        public bool Approved { get; set; }
    }
    
    public class Inspection
    {
        public Guid Id { get; set; }
        public DateTime InspectionDate { get; set; }
        public string Inspector { get; set; }
        public int Bags { get; set; }
        public List<Analysis> Analyses { get; set; } = new();
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