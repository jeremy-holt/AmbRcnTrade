using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Models;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.RavenIndexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IAppUserService
    {
        Task<AppUserInfo> GetCustomersForAppUser(string appUserId);
        Task<ServerResponse<List<AppUserPassword>>> SaveAppUsersPasswords(List<AppUserPassword> list);
        Task<List<AppUserPassword>> LoadAppUsersPasswords();
        Task<ServerResponse<AppUserPassword>> SaveAppUserPassword(AppUserDto appUserDto);
    }

    public class AppUserService : IAppUserService
    {
        private readonly IAsyncDocumentSession _session;

        public AppUserService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<AppUserInfo> GetCustomersForAppUser(string appUserId)
        {
            var userCustomers = await _session.Query<Customers_ByAppUserId.Result, Customers_ByAppUserId>()
                .Include(c => c.AppUserId)
                .Where(c => c.AppUserId == appUserId)
                .Select(c => c.CustomerId)
                .ToListAsync();

            var appUser = await _session.LoadAsync<AppUser>(appUserId);

            return new AppUserInfo {UserCustomerIds = userCustomers, AppUserId = appUserId, AppUserName = appUser.Name, AppUserRole = appUser.Role};
        }

        public async Task<ServerResponse<List<AppUserPassword>>> SaveAppUsersPasswords(List<AppUserPassword> list)
        {
            var users = await _session.Query<AppUserPassword>().ToListAsync();

            foreach (var user in list)
            {
                var entity = users.FirstOrDefault(c => c.Id == user.Id) ?? new AppUserPassword {Email = user.Email, Name = user.Name, Password = user.Password};
                await _session.StoreAsync(entity);
                user.Id = entity.Id;
            }

            return new ServerResponse<List<AppUserPassword>>(list, "Saved");
        }

        public async Task<List<AppUserPassword>> LoadAppUsersPasswords()
        {
            return await _session.Query<AppUserPassword>().ToListAsync();
        }

        public async Task<ServerResponse<AppUserPassword>> SaveAppUserPassword(AppUserDto appUserDto)
        {
            var appUserPassword = new AppUserPassword
            {
                Email = appUserDto.Email,
                Password = appUserDto.Password,
                Name = appUserDto.Name,
                AppUserId = appUserDto.Id
            };
            await _session.StoreAsync(appUserPassword);
            await _session.SaveChangesAsync();
            return new ServerResponse<AppUserPassword>(appUserPassword, "Saved");
        }
    }
}