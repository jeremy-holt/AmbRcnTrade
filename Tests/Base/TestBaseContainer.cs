using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using AmberwoodCore.Testing;
using AmbRcnTradeServer.Models.AttachmentModels;
using AmbRcnTradeServer.Services;
using AmbRcnTradeServer.Startup_Config;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Xunit.Abstractions;

namespace Tests.Base
{
    public class TestBaseContainer : TestsBase
    {
        protected const string COMPANY_ID = "companies/1-A";

        public TestBaseContainer(ITestOutputHelper output) : base(output, new ClientMappingProfile()) { }

        protected ICounterService GetCounterService(IAsyncDocumentSession session)
        {
            return new CounterService(session);
        }

        protected HttpRequest FakeRequest(IIdentity contract, List<PostAttachmentRequest> requests)
        {
            IFormFileCollection files = new FormFileCollection
            {
                requests[0].File,
                requests[1].File,
                requests[2].File
            };

            IFormCollection form = new FormCollection(null, files);

            var httpRequest = A.Fake<HttpRequest>();

            var header = new HeaderDictionary
            {
                new("id", new StringValues(contract.Id))
            };

            A.CallTo(() => httpRequest.Headers).Returns(header);
            A.CallTo(() => httpRequest.ReadFormAsync(default)).Returns(form);
            A.CallTo(() => httpRequest.Scheme).Returns("https");
            A.CallTo(() => httpRequest.Host).Returns(new HostString("localhost", 8080));

            return httpRequest;
        }

        protected IAppUserService GetAppUserService(IAsyncDocumentSession session)
        {
            return new AppUserService(session);
        }

        protected IInspectionService GetInspectionService(IAsyncDocumentSession session)
        {
            return new InspectionService(session);
        }

        protected IStockService GetStockService(IAsyncDocumentSession session)
        {
            return new StockService(session, new CounterService(session), GetInspectionService(session));
        }

        protected IPurchaseService GetPurchaseService(IAsyncDocumentSession session)
        {
            return new PurchaseService(session, new CounterService(session), GetInspectionService(session), GetStockService(session));
        }

        protected IStockManagementService GetStockManagementService(IAsyncDocumentSession session)
        {
            return new StockManagementService(session, GetStockService(session), GetInspectionService(session));
        }

        protected IContainerService GetContainerService(IAsyncDocumentSession session)
        {
            return new ContainerService(session);
        }

        protected IVesselService GetVesselService(IAsyncDocumentSession session)
        {
            return new VesselService(session, Mapper);
        }

        protected IBillLadingService GetBillLadingService(IAsyncDocumentSession session)
        {
            return new BillLadingService(session, Mapper);
        }

        protected IPaymentService GetPaymentService(IAsyncDocumentSession session)
        {
            return new PaymentService(session, GetPurchaseService(session), GetCounterService(session));
        }

        protected IDraftBillLadingService GetDraftBillLadingService(IAsyncDocumentSession session)
        {
            return new DraftBillLadingService(session, GetBillLadingService(session));
        }

        protected IBagDeliveryService GetBagDeliveryService(IAsyncDocumentSession session)
        {
            return new BagDeliveryService(session);
        }
    }
}