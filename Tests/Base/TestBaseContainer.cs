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

        // protected ICounterService GetCounterService(IAsyncDocumentSession session)
        // {
        //     return new CounterService(session);
        // }
        //
        // protected IContractService GetContractService(IAsyncDocumentSession session)
        // {
        //     return new ContractService(session, GetCounterService(session), GetAppUserService(session), GetContractDocumentService(session));
        // }
        //
        // protected IChartOfAccountsService GetChartOfAccountsService(IAsyncDocumentSession session)
        // {
        //     return new ChartOfAccountsService(session, GetTransactionService(session));
        // }
        //
        // protected ITransactionService GetTransactionService(IAsyncDocumentSession session)
        // {
        //     return new TransactionService(session, GetTransactionHelperService(session));
        // }
        //
        // protected ITransactionHelperService GetTransactionHelperService(IAsyncDocumentSession session)
        // {
        //     return new TransactionHelperService(session);
        // }
        //
        // protected IToleranceService GetToleranceService(IAsyncDocumentSession session)
        // {
        //     return new ToleranceService(session, GetGradesService(session));
        // }
        //
        // protected static IGradesService GetGradesService(IAsyncDocumentSession session)
        // {
        //     return new GradesService(session);
        // }
        //
        // protected IAppUserService GetAppUserService(IAsyncDocumentSession session)
        // {
        //     return new AppUserService(session);
        // }
        //
        // protected IAuditingService GetAuditingService(IAsyncDocumentSession session)
        // {
        //     return new AuditingService(session);
        // }
        //
        // protected IContractDocumentService GetContractDocumentService(IAsyncDocumentSession session)
        // {
        //     return new ContractDocumentService(session, GetAttachmentService(session));
        // }
        //
        // protected IAttachmentService GetAttachmentService(IAsyncDocumentSession session)
        // {
        //     return new AttachmentService(session);
        // }

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
    }
}