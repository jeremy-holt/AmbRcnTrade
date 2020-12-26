using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Serilog;

namespace AmbRcnTradeServer.Startup_Config
{
    public static class DependencyInjection
    {
        public static void Configure(IServiceCollection services)
        {
            AmberwoodCore.Initializations.DependencyInjection.Configure(services);

            // services.AddScoped<ICommodityGroupService, CommodityGroupService>();
            // services.AddScoped<ICommodityService, CommodityService>();
            // services.AddScoped<IContractClauseService, ContractClauseService>();
            // services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ICounterService, CounterService>();
            services.AddScoped<ICustomerService, CustomerService>();
            // services.AddScoped<IGradesService, GradesService>();
            // services.AddScoped<IIncotermsService, IncotermsService>();
            // services.AddScoped<IPackingService, PackingService>();
            // services.AddScoped<IPaymentService, PaymentService>();
            // services.AddScoped<IPortsService, PortsService>();
            // services.AddScoped<IStandardClauseService, StandardClauseService>();
            // services.AddScoped<ITransportUnitService, TransportUnitService>();
            // services.AddScoped<IClauseGroupService,ClauseGroupService>();
            // services.AddScoped<IChartOfAccountsService, ChartOfAccountsService>();
            // services.AddScoped<ITransactionService, TransactionService>();
            // services.AddScoped<IDefaultDataService, DefaultDataService>();
            // services.AddScoped<ITransactionHelperService, TransactionHelperService>();
            // services.AddScoped<IInspectionService, InspectionService>();
            // services.AddScoped<ISpecificationService, SpecificationService>();
            // services.AddScoped<IToleranceDescriptorService, ToleranceDescriptorService>();
            // services.AddScoped<IToleranceService, ToleranceService>();
            // services.AddScoped<IAppUserService, AppUserService>();
            // services.AddScoped<IAuditingService, AuditingService>();
            // services.AddScoped<IAttachmentService, AttachmentService>();
            // services.AddScoped<IContractDocumentService, ContractDocumentService>();
            
            Log.Logger.Information("Successfully ran DependencyInjection.Configure()");
        }
    }
}