using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.DictionaryModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Contracts_ByCustomer : AbstractMultiMapIndexCreationTask<Contracts_ByCustomer.Result>
    {
        public Contracts_ByCustomer()
        {
            AddMap<Contract>(contracts => from c in contracts
                let customer = LoadDocument<Customer>(c.SellerId)
                select new
                {
                    CustomerId = c.SellerId,
                    CustomerName = customer.Name
                });

            AddMap<Contract>(contracts => from c in contracts
                let customer = LoadDocument<Customer>(c.BuyerId)
                select new
                {
                    CustomerId = c.BuyerId,
                    CustomerName = customer.Name
                });

            AddMap<Contract>(contracts => from c in contracts
                let customer = LoadDocument<Customer>(c.BrokerId)
                select new
                {
                    CustomerId = c.BrokerId,
                    CustomerName = customer.Name
                });

            Index(x => x.CustomerId, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }

        public class Result
        {
            public string CustomerId { get; set; }
            public string CustomerName { get; set; }
        
    }
    }
}