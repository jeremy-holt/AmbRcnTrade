﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.RavenIndexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface ICustomerService
    {
        Task<ServerResponse<Customer>> SaveCustomer(Customer customer);
        Task<Customer> LoadCustomer(string id);
        Task<List<Customer>> LoadCustomerList(string companyId);
        Task<ServerResponse<Customer>> AddUser(string customerId, AppUser appUser);
        Task<List<CustomerUserListItem>> ListCustomersAndUsers(string companyId);
        Task<List<ListItem>> LoadCustomerListForAppUser(string companyId, string appUserId);
    }

    public class CustomerService : ICustomerService
    {
        private readonly IAsyncDocumentSession _session;

        public CustomerService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<ServerResponse<Customer>> SaveCustomer(Customer customer)
        {
            await _session.StoreAsync(customer);
            return new ServerResponse<Customer>(customer, $"Saved {customer.Name}");
        }

        public async Task<Customer> LoadCustomer(string id)
        {
            return await _session.LoadAsync<Customer>(id);
        }

        public async Task<List<Customer>> LoadCustomerList(string companyId)
        {
            var query = await _session.Query<Customer>()
                .Where(c => c.CompanyId == companyId)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return query;
        }

        public async Task<ServerResponse<Customer>> AddUser(string customerId, AppUser appUser)
        {
            var customer = await _session.LoadAsync<Customer>(customerId);

            var found = customer.Users.FirstOrDefault(c => c.AppUserId == appUser.Id);
            if (found == null)
                customer.Users.Add(new User {AppUserId = appUser.Id, Name = appUser.Name});
            else
                found.Name = appUser.Name;

            await _session.StoreAsync(customer);

            return new ServerResponse<Customer>(customer, $"Added {appUser.Name} to {customer.Name}");
        }

        public async Task<List<CustomerUserListItem>> ListCustomersAndUsers(string companyId)
        {
            var customers = await _session.Query<Customer>()
                .Where(c => c.CompanyId == companyId)
                .OrderBy(c => c.Name)
                .Select(c => new CustomerUserListItem {CustomerId = c.Id, Name = c.Name, CustomerName = c.CompanyName, Users = c.Users})
                .ToListAsync();

            var users = await _session.Query<AppUser>()
                .OrderBy(c => c.Name)
                .Select(c => new {AppUserId = c.Id, c.Name})
                .ToListAsync();

            var list = new List<CustomerUserListItem>();

            foreach (var customer in customers)
            {
                var item = new CustomerUserListItem
                {
                    CustomerId = customer.CustomerId,
                    CustomerName = customer.CustomerName,
                    Name = customer.Name,
                    Users = users.Where(c => c.AppUserId.In(customer.Users.Select(x => x.AppUserId))).Select(p => new User(p.AppUserId, p.Name)).ToList()
                };
                list.Add(item);
            }

            return list;
        }

        public async Task<List<ListItem>> LoadCustomerListForAppUser(string companyId, string appUserId)
        {
            if (appUserId.IsNullOrEmpty())
            {
                return await _session.Query<Customer>()
                    .Where(c => c.CompanyId == companyId)
                    .OrderBy(c => c.Name)
                    .Select(c => new ListItem {Id = c.Id, Name = c.Name})
                    .ToListAsync();
            }

            var query = await _session.Query<Contracts_ByAppUser.Result, Contracts_ByAppUser>()
                .Where(c => c.CompanyId == companyId && c.Users.Contains(appUserId))
                .ProjectInto<Contracts_ByAppUser.Result>()
                .ToListAsync();
            
            return query.Where(c => c.Id.IsNotNullOrEmpty())
                .Select(c => new ListItem(c.CustomerId, c.CustomerName, c.CompanyId))
                .DistinctBy(c => c.Id)
                .OrderBy(c => c.Name)
                .ToList();
        }
    }
}