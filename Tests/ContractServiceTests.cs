using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.RavenIndexes;
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
    public class ContractServiceTests : TestBaseContainer
    {
        public ContractServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Load_ShouldLoadAPurchase()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContractService(session);
            var fixture = new Fixture();

            var contract = fixture.DefaultEntity<Contract>().Create();
            await session.StoreAsync(contract);
            await session.SaveChangesAsync();

            // Act
            var actual = await sut.Load(contract.Id);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadContainersList_ShouldLoadListForAppUser()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContractService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var appUser = fixture.DefaultIdentity<AppUser>().Create();
            await session.StoreAsync(appUser);

            var users = new List<User>
            {
                new(appUser.Id, appUser.Name)
            };
            var customerTerraNova = fixture.DefaultEntity<Customer>()
                .With(c => c.Users, users)
                .Create();

            var otherCustomers = fixture.DefaultEntity<Customer>().CreateMany(2).ToList();

            var contract = fixture.DefaultEntity<Contract>()
                .With(c => c.SellerId, customerTerraNova.Id)
                .With(c => c.BuyerId, otherCustomers[0].Id)
                .Without(c => c.BrokerId)
                .Create();
            await session.StoreAsync(contract);
            await session.SaveChangesAsync();
            
            WaitForIndexing(store);

            // Act
            var prms = new ContractQueryParameters
            {
                AppUserId = appUser.Id,
                CompanyId = COMPANY_ID
            };
            var list = await sut.LoadContainersList(prms);

            // Assert
            var actual = list.First();
            actual.Id.Should().Be(contract.Id);
            actual.ContractNumber.Should().Be(contract.ContractNumber);
            actual.ContractDate.Should().Be(contract.ContractDate);
            actual.Seller.Should().Be(new ListItem(customerTerraNova.Id, customerTerraNova.Name));
            actual.Buyer.Should().Be(new ListItem(otherCustomers[0].Id, otherCustomers[0].Name));
        }

        private static async Task InitializeIndexes(IDocumentStore store)
        {
            await new Customers_ByAppUserId().ExecuteAsync(store);
            await new Contract_ByContainers().ExecuteAsync(store);
        }

        [Fact]
        public async Task Save_ShouldLoadAPurchase()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContractService(session);
            var fixture = new Fixture();

            await session.StoreAsync(new Company {Id = COMPANY_ID});

            var purchase = fixture.DefaultEntity<Contract>()
                .Create();

            // Act
            var actual = await sut.Save(purchase);

            // Assert
            actual.Dto.ContractNumber.Should().Be("000001");

            await sut.Save(purchase);
            purchase.ContractNumber.Should().Be("000001");
        }
    }
}