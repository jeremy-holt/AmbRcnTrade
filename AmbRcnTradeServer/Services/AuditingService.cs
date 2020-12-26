using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IAuditingService
    {
        Task Log(HttpRequest httpRequest);
        Task<List<AuditLog>> LoadList();
        Task<ServerResponse> ClearLogs(int days);
    }

    public class AuditingService : IAuditingService
    {
        private readonly IAsyncDocumentSession _session;

        public AuditingService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task Log(HttpRequest httpRequest)
        {
            var claims = httpRequest.HttpContext.User.Claims.ToList();

            var auditLog = new AuditLog
            {
                Date = DateTime.Now,
                Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                AppUserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value,
                UserName = $"{claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value} {claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value}",
                UserRole = $"{claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value}",
                Page = httpRequest.Path,
                QueryString = Uri.UnescapeDataString(httpRequest.QueryString.ToString()),
                IpAddress = httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            if (auditLog.UserRole != "admin")
                await _session.StoreAsync(auditLog);
        }

        public async Task<List<AuditLog>> LoadList()
        {
            return await Queryable.OrderByDescending(_session.Query<AuditLog>(), c => c.Date)
                .ToListAsync();
        }

        public async Task<ServerResponse> ClearLogs(int days)
        {
            var query = days > 0
                ? await Queryable.Where(_session.Query<AuditLog>(), c => c.Date <= DateTime.Today.AddDays(-days)).ToListAsync()
                : await _session.Query<AuditLog>().ToListAsync();

            foreach (var log in query)
                _session.Delete(log);

            await _session.SaveChangesAsync();
            return new ServerResponse($"Removed logs for the last {days} days");
        }
    }
}