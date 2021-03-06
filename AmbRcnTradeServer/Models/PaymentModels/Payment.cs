﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.PurchaseModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.PaymentModels
{
    public class Payment : IEntityCompany
    {
        public DateTime? PaymentDate { get; set; }
        public string BeneficiaryId { get; set; }
        public double Value { get; set; }
        public Currency Currency { get; set; }
        public double ExchangeRate { get; set; }
        public string SupplierId { get; set; }
        public string Notes { get; set; }
        public long PaymentNo { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
    }

    public class PaymentDto
    {
        public List<PaymentListItem> PaymentList { get; set; } = new();

        public List<PurchaseListItem> PurchaseList { get; set; } = new();
        public double PurchaseValue { get; set; }
        public double PurchaseValueUsd { get; set; }
        public double PaymentValue { get; set; }
        public double PaymentValueUsd { get; set; }
    }

    public class PaymentListItem
    {
        public string Id { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string BeneficiaryId { get; set; }
        public double Value { get; set; }
        public Currency Currency { get; set; }
        public double ExchangeRate { get; set; }
        public string SupplierId { get; set; }
        public string BeneficiaryName { get; set; }
        public string SupplierName { get; set; }
        public string CompanyId { get; set; }
        public long PaymentNo { get; set; }
        public double ValueUsd { get; set; }
    }
}