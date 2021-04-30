using System;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.PaymentModels;
using AmbRcnTradeServer.Models.PurchaseModels;
using AmbRcnTradeServer.Models.StockModels;
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
    public class PaymentsServiceTests : TestBaseContainer
    {
        public PaymentsServiceTests(ITestOutputHelper output) : base(output) { }

        private static async Task InitializeIndexes(IDocumentStore store)
        {
            await new Stocks_ById().ExecuteAsync(store);
            await new Payments_ById().ExecuteAsync(store);
            await new Inspections_ByAnalysisResult().ExecuteAsync(store);
        }

        [Fact]
        public async Task DeletePayment_ShouldDeletePayment()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPaymentService(session);

            var payment = await new Payment().CreateAndStoreAsync(session);
            await session.SaveChangesAsync();

            // Act
            var response = await sut.DeletePayment(payment.Id);
            await session.SaveChangesAsync();

            // Assert
            response.Message.Should().Be("Deleted payment");

            using var session2 = store.OpenAsyncSession();
            var actual = await session2.LoadAsync<Payment>(payment.Id);
            actual.Should().BeNull();
        }


        [Fact]
        public async Task Load_ShouldLoadPayment()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetPaymentService(session);
            var fixture = new Fixture();

            var supplier = await new Customer().CreateAndStoreAsync(session);
            var beneficiary = await new Customer().CreateAndStoreAsync(session);

            var inspection = await new Inspection().CreateAndStoreAsync(session);

            var stocks = fixture.DefaultEntity<Stock>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.Bags, 1000)
                .With(c => c.LotNo, 1)
                .With(c => c.AnalysisResult, new AnalysisResult())
                .With(c => c.WeightKg, 80000)
                .With(c => c.IsStockIn, true)
                .With(c => c.InspectionId, inspection.Id)
                .CreateMany()
                .ToList();
            await stocks.SaveList(session);

            var purchaseDetails = fixture.Build<PurchaseDetail>()
                .With(c => c.PricePerKg, 400)
                .With(c => c.Currency, Currency.CFA)
                .With(c => c.ExchangeRate, 535)
                .With(c => c.StockIds, stocks.GetPropertyFromList(c => c.Id))
                .CreateMany()
                .ToList();

            var purchase = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, purchaseDetails)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await session.StoreAsync(purchase);

            var payment = fixture.DefaultEntity<Payment>()
                .With(c => c.BeneficiaryId, beneficiary.Id)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await session.StoreAsync(payment);

            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            var actual = await sut.Load(payment.Id);

            // Assert
            actual.Should().BeEquivalentTo(payment);
        }

        [Fact]
        public async Task LoadList_ShouldLoadListOfPayments()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetPaymentService(session);
            var fixture = new Fixture();

            var supplier = await new Customer().CreateAndStoreAsync(session);
            var beneficiary = await new Customer().CreateAndStoreAsync(session);

            var stocks = fixture.DefaultEntity<Stock>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.Bags, 1000)
                .With(c => c.LotNo, 1)
                .With(c => c.AnalysisResult, new AnalysisResult())
                .With(c => c.WeightKg, 80000)
                .With(c => c.IsStockIn, true)
                .CreateMany()
                .ToList();

            var purchaseDetails = fixture.Build<PurchaseDetail>()
                .With(c => c.PricePerKg, 400)
                .With(c => c.Currency, Currency.CFA)
                .With(c => c.ExchangeRate, 535)
                .With(c => c.StockIds, stocks.GetPropertyFromList(c => c.Id))
                .CreateMany()
                .ToList();

            var purchase = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, purchaseDetails)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await session.StoreAsync(purchase);

            var payments = fixture.DefaultEntity<Payment>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.BeneficiaryId, beneficiary.Id)
                .CreateMany().ToList();
            await payments.SaveList(session);

            // Act
            var list = await sut.LoadList(COMPANY_ID, null);

            // Assert
            list.Should().Contain(c => c.SupplierName == supplier.Name);
            list.Should().Contain(c => c.BeneficiaryName == beneficiary.Name);
            list.Should().Contain(c => c.PaymentNo > 0);
            list.Should().Contain(c => Math.Abs(c.ValueUsd - c.Value / c.ExchangeRate) < .01);
            list.Should().HaveCount(3);
        }

        [Fact]
        public async Task LoadPaymentsPurchases_ShouldLoadListForSupplierId()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetPaymentService(session);
            var fixture = new Fixture();

            var supplier = await new Customer().CreateAndStoreAsync(session);
            var beneficiary = await new Customer().CreateAndStoreAsync(session);

            var inspection = await new Inspection().CreateAndStoreAsync(session);

            var stocks = fixture.DefaultEntity<Stock>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.Bags, 1000)
                .With(c => c.LotNo, 1)
                .With(c => c.AnalysisResult, new AnalysisResult())
                .With(c => c.WeightKg, 80000)
                .With(c => c.IsStockIn, true)
                .With(c => c.InspectionId, inspection.Id)
                .CreateMany()
                .ToList();
            await stocks.SaveList(session);

            var purchaseDetails = fixture.Build<PurchaseDetail>()
                .With(c => c.PricePerKg, 400)
                .With(c => c.Currency, Currency.CFA)
                .With(c => c.ExchangeRate, 535)
                .With(c => c.StockIds, stocks.GetPropertyFromList(c => c.Id))
                .CreateMany()
                .ToList();

            var purchase = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, purchaseDetails)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await session.StoreAsync(purchase);

            var payment = fixture.DefaultEntity<Payment>()
                .With(c => c.BeneficiaryId, beneficiary.Id)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await session.StoreAsync(payment);

            await session.SaveChangesAsync();
            WaitForIndexing(store);

            var actual = await sut.LoadPaymentsPurchasesList(COMPANY_ID, payment.SupplierId);

            // Assert

            actual.PaymentList.Should().HaveCount(1);
            actual.PaymentList[0].Id.Should().Be(payment.Id);
            actual.PurchaseValue.Should().BeGreaterThan(0);
            actual.PurchaseValueUsd.Should().BeGreaterThan(0);
            // actual.PaymentValue.Should().BeGreaterThan(0);
            // actual.PaymentValueUsd.Should().BeGreaterThan(0);

            actual.PurchaseList.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task Save_ShouldSaveAPaymentDto()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetPaymentService(session);
            var fixture = new Fixture();

            await new Company().CreateIdAndStoreAsync(session);

            var beneficiary = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(beneficiary);

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var payment = fixture.DefaultEntity<Payment>()
                .With(c => c.BeneficiaryId, beneficiary.Id)
                .With(c => c.SupplierId, beneficiary.Id)
                .With(c => c.Value, 500_000)
                .With(c => c.ExchangeRate, 535)
                .With(c => c.Currency, Currency.CFA)
                .With(c => c.PaymentDate, DateTime.Today)
                .With(c => c.Notes, "notes")
                .Without(c => c.PaymentNo)
                .Create();

            // Act
            var response = await sut.Save(payment);
            await session.SaveChangesAsync();

            // Assert
            var actual = await session.LoadAsync<Payment>(response.Dto.Id);
            actual.Should().NotBeNull();
            actual.PaymentNo.Should().Be(1);
        }
    }
}