namespace AmbRcnTradeServer.Models.DictionaryModels
{
    public class CustomerListItem
    {
        public CustomerListItem(string id, string name, string companyId, string customerGroupId)
        {
            Id = id;
            Name = name;
            CompanyId = companyId;
            CustomerGroupId = customerGroupId;
        }

        public CustomerListItem() { }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; }
        public string CustomerGroupId { get; set; }
    }
}