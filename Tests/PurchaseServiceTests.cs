﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
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
    public class PurchaseServiceTests : TestBaseContainer
    {
        public PurchaseServiceTests(ITestOutputHelper output) : base(output) { }

        private static async Task InitializeIndexes(IDocumentStore store)
        {
            await new Inspections_ByAnalysisResult().ExecuteAsync(store);
            await new Stocks_ById().ExecuteAsync(store);
            // await new Purchases_ById().ExecuteAsync(store);
        }

        [Fact]
        public async Task Load_ShouldLoadPurchase()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPurchaseService(session);
            var inspectionService = GetInspectionService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            // var analysisResult = fixture.Build<AnalysisResult>().Create();

            var inspection = fixture.DefaultEntity<Inspection>()
                .Without(c => c.AnalysisResult)
                .Create();
            await inspectionService.Save(inspection);


            var stocks = fixture.DefaultEntity<Stock>()
                .With(c => c.LocationId, location.Id)
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.InspectionId, inspection.Id)
                .With(c => c.IsStockIn, true)
                .Without(c => c.AnalysisResult)
                .CreateMany().ToList();
            stocks[2].IsStockIn = false;

            await stocks.SaveList(session);
            await session.SaveChangesAsync();

            var purchaseDetails = fixture.Build<PurchaseDetail>()
                .With(c => c.StockIds, stocks.Select(x => x.Id).ToList)
                .Without(c => c.Stocks)
                .CreateMany()
                .ToList();

            var purchase = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, purchaseDetails)
                .Create();
            await session.StoreAsync(purchase);
            await session.SaveChangesAsync();

            WaitForIndexing(store);

            // Act
            var actual = await sut.Load(purchase.Id);

            // Assert
            actual.Should().NotBeNull();
            actual.Value.Should().Be(purchase.Value);

            foreach (var stock in actual.PurchaseDetails[0].Stocks)
            {
                stock.LocationName.Should().Be(location.Name);
                stock.SupplierName.Should().Be(supplier.Name);
                stock.IsStockIn.Should().BeTrue();
            }
        }

        [Fact]
        public async Task LoadList_ShouldLoadListOfPurchases()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPurchaseService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var supplier1 = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier1);

            var supplier2 = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier2);

            var inspection = fixture.DefaultEntity<Inspection>().Create();
            await session.StoreAsync(inspection);

            var stocks = fixture.DefaultEntity<Stock>()
                .With(c => c.InspectionId, inspection.Id)
                .Without(c => c.AnalysisResult)
                .CreateMany().ToList();
            await stocks.SaveList(session);

            var purchaseDetails = fixture.Build<PurchaseDetail>()
                .With(c => c.StockIds, stocks.Select(x => x.Id).ToList)
                .CreateMany()
                .ToList();

            var purchase1 = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, purchaseDetails)
                .With(c => c.SupplierId, supplier1.Id)
                .With(c => c.PurchaseNumber, 7)
                .Create();
            await session.StoreAsync(purchase1);

            var purchase2 = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, purchaseDetails)
                .With(c => c.SupplierId, supplier2.Id)
                .With(c => c.PurchaseNumber, 7)
                .Create();
            await session.StoreAsync(purchase2);

            await session.SaveChangesAsync();

            // Act
            var list = await sut.LoadList(COMPANY_ID, supplier1.Id);

            // Assert
            list.Should().Contain(c => c.SupplierId == purchase1.SupplierId);
            list.Should().OnlyContain(c => c.SupplierId == supplier1.Id);
            list.Should().Contain(c => c.SupplierName == supplier1.Name);
            list.Should().Contain(c => c.Id == purchase1.Id);
            list.Should().Contain(c => c.PurchaseNumber == purchase1.PurchaseNumber);
            list.Should().Contain(c => c.PurchaseDate == purchase1.PurchaseDate);
            list[0].Value.Should().Be(purchase1.Value);
            list[0].ValueUsd.Should().Be(purchase1.ValueUsd);

            var details = list[0].PurchaseDetails;
            details.Should().HaveCount(3);
            details[0].PricePerKg.Should().Be(purchaseDetails[0].PricePerKg);
            details[0].Currency.Should().Be(purchaseDetails[0].Currency);
            details[0].StockIds.Should().BeEquivalentTo(purchaseDetails[0].StockIds);
            details[0].Stocks[0].BagsIn.Should().Be(stocks[0].Bags);
            details[0].Value.Should().BeGreaterThan(0);
            details[0].ValueUsd.Should().BeGreaterThan(0);
        }

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
                QuantityMt = 200.0,
                DeliveryDate = new DateTime(2013, 1, 1),
                PurchaseDetails = new List<PurchaseDetail>
                {
                    new()
                    {
                        StockIds = stocks.Select(x => x.Id).ToList(),
                        PricePerKg = 400.0,
                        Currency = Currency.CFA,
                        ExchangeRate = 555.0,
                        PriceAgreedDate = new DateTime(2013, 1, 1)
                    }
                }
            };

            // Act
            var response = await sut.Save(purchase);

            // Assert
            var actual = await session.LoadAsync<Purchase>(response.Id);
            actual.Should().NotBeNull();
            actual.PurchaseNumber.Should().Be(1L);
            actual.QuantityMt.Should().Be(200);
            actual.DeliveryDate.Should().Be(new DateTime(2013, 1, 1));
            actual.PurchaseDetails[0].Value.Should().Be(400.0 * stocks.Sum(x => x.WeightKg));
            actual.PurchaseDetails[0].ValueUsd.Should().Be(400.0 * stocks.Sum(x => x.WeightKg) / 555);
            actual.Value.Should().Be(actual.PurchaseDetails.Sum(x => x.Value));
            actual.ValueUsd.Should().Be(actual.PurchaseDetails.Sum(x => x.ValueUsd));
        }
    }
}