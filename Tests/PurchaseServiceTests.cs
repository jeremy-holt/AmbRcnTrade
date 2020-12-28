using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.PurchaseModels;
using AmbRcnTradeServer.Models.StockModels;
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
    public class PurchaseServiceTests : TestBaseContainer
    {
        public PurchaseServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Save_ShouldSaveAPurchase()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPurchaseService(session);
            var fixture = new Fixture();

            await session.StoreAsync(new Company(COMPANY_ID));

            var stocks = fixture.DefaultEntity<Stock>().CreateMany().ToList();
            await stocks.SaveList(session);

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var purchase = new Purchase
            {
                Id = null,
                PurchaseNumber = default,
                CompanyId = COMPANY_ID,
                SupplierId = supplier.Id,
                PurchaseDetails = new List<PurchaseDetail>
                {
                    new() {StockIds = stocks.Select(x => x.Id).ToList(), PricePerKg = 400.0, Currency = Currency.CFA, ExchangeRate = 555.0, Date = new DateTime(2013, 1, 1)}
                }
            };

            // Act
            var response = await sut.Save(purchase);

            // Assert
            var actual = await session.LoadAsync<Purchase>(response.Id);
            actual.Should().NotBeNull();
            actual.PurchaseNumber.Should().Be(1L);
        }

        [Fact]
        public async Task Load_ShouldLoadPurchase()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPurchaseService(session);
            var fixture = new Fixture();

            var stocks = fixture.DefaultEntity<Stock>().CreateMany().ToList();
            await stocks.SaveList(session);
            await session.SaveChangesAsync();

            var purchases = fixture.Build<PurchaseDetail>()
                .With(c => c.StockIds, stocks.Select(x => x.Id).ToList)
                .CreateMany()
                .ToList();

            var purchase = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, purchases)
                .Create();
            await session.StoreAsync(purchase);
            await session.SaveChangesAsync();
            
            WaitForIndexing(store);
            
            // Act
            Purchase actual = await sut.Load(purchase.Id);

            // Assert
            actual.Should().NotBeNull();

            foreach (var stock in actual.PurchaseDetails[0].Stocks)
            {
                var foundStock = stocks.FirstOrDefault(c => c.Id == stock.Id);
                foundStock.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task LoadList_ShouldLoadListOfPurchases()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPurchaseService(session);
            var fixture = new Fixture();

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var stocks = fixture.DefaultEntity<Stock>().CreateMany().ToList();
            await stocks.SaveList(session);

            var purchaseDetails = fixture.Build<PurchaseDetail>()
                .With(c => c.StockIds, stocks.Select(x => x.Id).ToList)
                .CreateMany()
                .ToList();

            var purchase = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, purchaseDetails)
                .With(c=>c.SupplierId,supplier.Id)
                .With(c => c.PurchaseNumber, 7)
                .Create();
            await session.StoreAsync(purchase);

            await session.SaveChangesAsync();
            
            // Act
            var list = await sut.LoadList(COMPANY_ID);

            // Assert
            list.Should().Contain(c => c.SupplierId == purchase.SupplierId);
            list.Should().Contain(c => c.SupplierName == supplier.Name);
            list.Should().Contain(c => c.Id == purchase.Id);
            list.Should().Contain(c => c.PurchaseNumber == purchase.PurchaseNumber);
            list.Should().Contain(c => c.PurchaseDate == purchase.PurchaseDate);
            list.Should().Contain(c => c.StockIn != null);
            list.Should().Contain(c => c.StockOut != null);
            list.Should().Contain(c => c.StockBalance != null);
        }
    }
}