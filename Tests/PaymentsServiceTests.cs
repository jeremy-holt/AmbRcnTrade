using System;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.Payments;
using AmbRcnTradeServer.Services;
using AutoFixture;
using FluentAssertions;
using Raven.Client.Documents.Session;
using Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class PaymentsServiceTests: TestBaseContainer
    {
        public PaymentsServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Save_ShouldSaveAPayment()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            IPaymentService sut = GetPaymentService(session);
            var fixture = new Fixture();

            var beneficiary = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(beneficiary);

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var payment = fixture.DefaultEntity<Payment>()
                .With(c => c.BeneficiaryId, beneficiary.Id)
                .With(c => c.SupplierId, beneficiary.Id)
                .With(c=>c.Value, 500_000)
                .With(c=>c.ExchangeRate,535)
                .With(c=>c.PaymentDate,DateTime.Today)
                .Create();
            
            // Act
            ServerResponse<Payment> response = await sut.Save(payment);
            
            // Assert
            var actual = await session.LoadAsync<Payment>(response.Id);
            actual.Should().NotBeNull();
        }
    }
}