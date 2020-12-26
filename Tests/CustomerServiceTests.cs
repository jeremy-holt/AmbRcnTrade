using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Services;
using AutoFixture;
using FluentAssertions;
using Raven.Client.Documents.Session;
using Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class CustomerServiceTests: TestBaseContainer
    {
        public CustomerServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Save_ShouldSaveACustomer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            ICustomerService sut = GetCustomerService(session);
            var fixture = new Fixture();

            var customer = fixture.DefaultEntity<Customer>().Create();
            
            // Act
            var response = await sut.SaveCustomer(customer);
            
            // Assert
            var actual = await session.LoadAsync<Customer>(response.Id);
            actual.Should().NotBeNull();
        }

        private ICustomerService GetCustomerService(IAsyncDocumentSession session)
        {
            return new CustomerService(session);
        }
    }
}