using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.AppUserModels;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.RavenIndexes;
using AmbRcnTradeServer.Services;
using AutoFixture;
using FluentAssertions;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class CustomerServiceTests : TestBaseContainer
    {
        public CustomerServiceTests(ITestOutputHelper output) : base(output) { }

        private static async Task InitializeIndexes(IDocumentStore store)
        {
            await new Customers_ByAppUserId().ExecuteAsync(store);
            await new Entities_ByAppUser().ExecuteAsync(store);
        }

        private ICustomerService GetCustomerService(IAsyncDocumentSession session)
        {
            return new CustomerService(session);
        }

        [Fact]
        public async Task Load_ShouldLoadCustomer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetCustomerService(session);
            var fixture = new Fixture();

            var customer = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(customer);

            // Act
            var actual = await sut.LoadCustomer(customer.Id);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadCustomerListForAppUser_ShouldLoadCustomersForAppUser()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetCustomerService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var appUser = fixture.DefaultIdentity<AppUser>().With(c=>c.Name,"Shalin").Create();
            await session.StoreAsync(appUser);

            var users = new List<User>
            {
                new(appUser.Id, appUser.Name)
            };
            
            var customerTerraNova = fixture.DefaultEntity<Customer>()
                .With(c => c.Users, users)
                .With(c=>c.Name,"Terra Nova")
                .Create();
            await session.StoreAsync(customerTerraNova);

            var customerGreenangle = fixture.DefaultEntity<Customer>()
                .Without(c => c.Users)
                .Create();
            await session.StoreAsync(customerGreenangle);

            var contract = fixture.DefaultEntity<Contract>()
                .With(c => c.SellerId, customerTerraNova.Id)
                .With(c => c.BuyerId, customerGreenangle.Id)
                .Create();
            await session.StoreAsync(contract);
            await session.SaveChangesAsync();

            WaitForIndexing(store);
            WaitForUserToContinueTheTest(store);

            // Act
            List<CustomerListItem> list = await sut.LoadCustomerListForAppUser(COMPANY_ID, appUser.Id, false);

            // Assert
            list.Should().OnlyContain(c => c.Id == customerTerraNova.Id);
            list[0].CustomerGroupId.Should().NotBeNull();
        }

        [Fact]
        public async Task Save_ShouldSaveACustomer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetCustomerService(session);
            var fixture = new Fixture();

            var customer = fixture.DefaultEntity<Customer>()
                .With(c=>c.CustomerGroupId, "CustomerGroups/1-A")
                .Create();

            // Act
            var response = await sut.SaveCustomer(customer);

            // Assert
            var actual = await session.LoadAsync<Customer>(response.Id);
            actual.Should().NotBeNull();
            actual.CustomerGroupId.Should().Be("CustomerGroups/1-A");
        }
    }
}