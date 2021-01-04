using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Services;
using AutoFixture;
using FluentAssertions;
using Raven.Client.Documents.Session;
using Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class CustomerGroupServiceTests: TestBaseContainer
    {
        public CustomerGroupServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Save_ShouldSaveCustomerGroup()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            ICustomerGroupService sut = GetCustomerGroupService(session);
            
            var customerGroup = new CustomerGroup("Suppliers");
            
            // Act
            ServerResponse<CustomerGroup> response = await sut.Save(customerGroup);
            var actual = await session.LoadAsync<CustomerGroup>(response.Id);

            // Assert
            actual.Name.Should().Be("Suppliers");
        }

        [Fact]
        public async Task Load_ShouldLoadCustmerGroup()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetCustomerGroupService(session);
            var fixture = new Fixture();

            var customerGroup = fixture.DefaultIdentity<CustomerGroup>().Create();
            await session.StoreAsync(customerGroup);
            
            // Act
            CustomerGroup actual = await sut.Load(customerGroup.Id);
            
            // Assert
            actual.Name.Should().Be(customerGroup.Name);
        }

        [Fact]
        public async Task LoadList_ShouldLoadList()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetCustomerGroupService(session);
            var fixture = new Fixture();

            var customerGroups = fixture.DefaultEntity<CustomerGroup>()
                .CreateMany().ToList();
            
            await customerGroups.SaveList(session);
            
            // Act
            List<CustomerGroup> list = await sut.LoadList(COMPANY_ID);
            
            // Assert
            list.Should().BeInAscendingOrder(c => c.Name);
        }
    }
}