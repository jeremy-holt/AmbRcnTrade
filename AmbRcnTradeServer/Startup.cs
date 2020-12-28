using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AmberwoodCore.Controllers;
using AmberwoodCore.Initializations;
using AmberwoodCore.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Serilog;
using DependencyInjection = AmbRcnTradeServer.Startup_Config.DependencyInjection;

namespace AmbRcnTradeServer
{
    public class Startup
    {
        private readonly Assembly _assembly = typeof(DependencyInjection).Assembly;
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            Configuration = configuration;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            JwtInitialize.Configure(services, EnvironmentVariablesHelper.Values());
            SwaggerInitialize.ConfigureServices(services);
            Automapper.Configure(services, _assembly);
            DependencyInjection.Configure(services);
            Globalization.Configure(services);

            RavenDbInitialize.Configure(services, _env.IsDevelopment(), _assembly, "AmbRcnTrade");

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    // options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
                    // options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
                });

            Log.Logger.Information("Successfully completed Startup.ConfigureServices()");
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseGlobalExceptionMiddleware();
            
            Globalization.Configure(app);

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".yaml"] = "application/x-yaml";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "locales")),
                RequestPath = "/locales",
                ContentTypeProvider = provider
            });
            
            app.UseFileServer(new FileServerOptions {EnableDefaultFiles = true, EnableDirectoryBrowsing = false});
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            SwaggerInitialize.Configure(app);
            
            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute().RequireAuthorization());

            Log.Logger.Information("Successfully ran Startup.Configure()");
        }
    }
}