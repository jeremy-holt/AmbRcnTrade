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
    public class Customers_ByAppUserId : AbstractIndexCreationTask<Customer, Customers_ByAppUserId.Result>
    {
        public Customers_ByAppUserId()
        {
            Map = customers => from c in customers
                from u in c.Users
                select new
                {
                    CustomerId = c.Id,
                    u.AppUserId,
                    c.CompanyId
                };
            Index(x => x.CustomerId, FieldIndexing.Default);
            Index(x => x.AppUserId, FieldIndexing.Default);
            Index(x=>x.CustomerId,FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }

        public class Result
        {
            public string CustomerId { get; set; }
            public string AppUserId { get; set; }
            public string CompanyId { get; set; }
        }
    }
}