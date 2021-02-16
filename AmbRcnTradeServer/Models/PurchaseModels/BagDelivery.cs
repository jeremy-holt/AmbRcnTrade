using System;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.PurchaseModels
{
    public class BagDelivery: IEntityCompany
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string SupplierId { get; set; }
        public int NumberBags { get; set; }
        public BagType BagType { get; set; }
        public string Notes { get; set; }
    }

    public class BagDeliveryListItem
    {
        public string Id { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string SupplierId { get; set; }
        public int NumberBags { get; set; }
        public BagType BagType { get; set; }
    }
}