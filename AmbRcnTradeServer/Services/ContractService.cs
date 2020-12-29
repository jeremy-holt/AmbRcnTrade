using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.RavenIndexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IContractService
    {
        Task<ServerResponse<Contract>> Save(Contract contract);
        Task<Contract> Load(string id);
        Task<List<ContractListItem>> LoadContainersList(ContractQueryParameters prms);
    }

    public class ContractService : IContractService
    {
        private readonly IAppUserService _appUserService;
        private readonly ICounterService _counterService;
        private readonly IAsyncDocumentSession _session;

        public ContractService(IAsyncDocumentSession session, ICounterService counterService, IAppUserService appUserService)
        {
            _session = session;
            _counterService = counterService;
            _appUserService = appUserService;
        }

        public async Task<ServerResponse<Contract>> Save(Contract contract)
        {
            if (contract.Id.IsNullOrEmpty())
            {
                var nextContractNumber = await _counterService.GetNextContractNumber(contract.CompanyId);
                contract.ContractNumber = nextContractNumber.ToString("D6");
            }

            await _session.StoreAsync(contract);
            return new ServerResponse<Contract>(contract, "Saved");
        }

        public async Task<Contract> Load(string id)
        {
            return await _session.LoadAsync<Contract>(id);
        }

        public async Task<List<ContractListItem>> LoadContainersList(ContractQueryParameters prms)
        {
            if (prms.AppUserId.IsNullOrEmpty())
                throw new InvalidOperationException("You must provide the AppUserId");

            var appUserInfo = await _appUserService.GetCustomersForAppUser(prms.AppUserId);

            var query = _session.Query<ContractListItem, Contract_ByContainers>()
                .Where(c => c.CompanyId == prms.CompanyId && (c.SellerId.In(appUserInfo.UserCustomerIds) || c.BuyerId.In(appUserInfo.UserCustomerIds))
                );

            if (prms.SellerId.IsNotNullOrEmpty())
                query = query.Where(c => c.SellerId == prms.SellerId);

            if (prms.BuyerId.IsNotNullOrEmpty())
                query = query.Where(c => c.BuyerId == prms.BuyerId);

            if (prms.ContainerStatus == null)
            {
                query = query.Where(c => c.Status != ContainerStatus.Cancelled);
            }
            else
            {
                query = prms.ContainerStatus switch
                {
                    ContainerStatus.Open => query.Where(c => c.Status == ContainerStatus.Open || c.Status == ContainerStatus.ShippedWithoutDocuments),
                    ContainerStatus.Shipped => query.Where(c => c.Status == ContainerStatus.Shipped || c.Status == ContainerStatus.ShippedWithoutDocuments),
                    _ => query.Where(c => c.Status == prms.ContainerStatus)
                };
            }

            var list = await query
                .ProjectInto<ContractListItem>()
                .ToListAsync();

            return list;
        }
    }
}