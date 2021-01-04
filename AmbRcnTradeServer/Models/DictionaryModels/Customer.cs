using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Models.AppUserModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.DictionaryModels
{
    public class Customer : IEntityCompany
    {
        public string CompanyName { get; set; }
        public Address Address { get; set; }
        public string Notes { get; set; }
        public string Telephone { get; set; }

        public List<User> Users { get; set; } = new();
        public string Id { get; set; }
        public string Name { get; set; }
        public string CompanyId { get; set; }
        public string CustomerGroupId { get; set; }
    }
}