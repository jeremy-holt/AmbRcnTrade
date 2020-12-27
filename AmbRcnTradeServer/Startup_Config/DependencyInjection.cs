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
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<ICounterService, CounterService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IPortsService, PortsService>();
            services.AddScoped<IInspectionService, InspectionService>();
            services.AddScoped<IAppUserService, AppUserService>();
            services.AddScoped<IAuditingService, AuditingService>();
           
            Log.Logger.Information("Successfully ran DependencyInjection.Configure()");
        }
    }
}