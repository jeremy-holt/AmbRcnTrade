using AmbRcnTradeServer.Constants;

namespace AmbRcnTradeServer.Models.DictionaryModels
{
    public class CustomerListItem
    {
        public CustomerListItem(string id, string name, string companyId, CustomerGroup filter)
        {
            Id = id;
            Name = name;
            CompanyId = companyId;
            Filter = filter;
        }

        public CustomerListItem() { }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; }
        public CustomerGroup Filter { get; set; }
    }
}