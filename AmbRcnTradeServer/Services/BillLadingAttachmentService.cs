using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.AttachmentModels;
using AmbRcnTradeServer.Models.VesselModels;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IBillLadingAttachmentService: IAttachmentService
    {
        Task<List<AttachmentInfo>> GetDocumentRoutes(HttpRequest httpRequest, string controllerName, string blLadingId);
    }

    public class BillLadingAttachmentService : AttachmentServiceBase, IBillLadingAttachmentService
    {
        public BillLadingAttachmentService(IAsyncDocumentSession session) : base(session) { }

        public async Task<List<AttachmentInfo>> GetDocumentRoutes(HttpRequest httpRequest, string controllerName, string blLadingId)
        {
            var routes = await GetAttachmentRoutes<BillLading>(httpRequest, controllerName, blLadingId);

            // var prefix = containerId == null ? "root!" : $"{containerId.ToString()}!";

            // var filteredRoutes = routes.Where(c => c.Name.StartsWith(prefix)).ToList();
            // foreach (var route in filteredRoutes)
            // {
            //     route.DisplayName = $"{route.Name.Split("!")[1]}";
            // }

            foreach (var route in routes)
            {
                route.DisplayName = route.Name;
            }

            return routes;
        }
    }
}