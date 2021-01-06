using System;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.Payments
{
    public class Payment: IEntityCompany
    {
        public DateTime? PaymentDate { get; set; }
        public string BeneficiaryId { get; set; }
        public double Value { get; set; }
        public Currency Currency { get; set; }
        public double ExchangeRate { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public string SupplierId { get; set; }
    }
}