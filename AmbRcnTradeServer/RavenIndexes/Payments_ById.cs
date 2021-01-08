using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.PaymentModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Payments_ById : AbstractIndexCreationTask<Payment, PaymentListItem>
    {
        public Payments_ById()
        {
            Map = payments => from c in payments
                select new PaymentListItem
                {
                    BeneficiaryId = c.BeneficiaryId,
                    SupplierId = c.SupplierId,
                    CompanyId = c.CompanyId
                };

            Index(x => x.BeneficiaryId, FieldIndexing.Default);
            Index(x => x.SupplierId, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }
    }
}