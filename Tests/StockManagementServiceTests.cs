﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Interfaces;
using AmbRcnTradeServer.Models.ContainerModels;
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
    public class StockManagementServiceTests : TestBaseContainer
    {
        public StockManagementServiceTests(ITestOutputHelper output) : base(output) { }

        private static async Task InitializeIndexes(IDocumentStore store)
        {
            await new Stocks_ByPurchases().ExecuteAsync(store);
            await new Inspections_ByAnalysisResult().ExecuteAsync(store);
        }


        [Fact]
        public async Task GetAvailableContainers_ShouldReturnEmptyOrStuffingContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockManagementService(session);
            var fixture = new Fixture();

            var container1 = fixture.DefaultEntity<Container>()
                .With(c => c.Status, ContainerStatus.Stuffing)
                .With(c => c.IncomingStocks, new List<IncomingStock>
                {
                    new() {Bags = 200, WeightKg = 16_000}
                })
                .With(c => c.Bags, 200)
                .With(c => c.StuffingWeightKg, 16_000)
                .Create();

            var container2 = fixture.DefaultEntity<Container>()
                .With(c => c.Status, ContainerStatus.Empty)
                .Without(c => c.IncomingStocks)
                .Without(c => c.Bags)
                .Without(c => c.StuffingWeightKg)
                .Create();

            var container3 = fixture.DefaultEntity<Container>()
                .With(c => c.Status, ContainerStatus.OnWayToPort)
                .Without(c => c.IncomingStocks)
                .Create();

            var container4 = fixture.DefaultEntity<Container>()
                .With(c => c.Status, ContainerStatus.Stuffing)
                .With(c => c.Bags, 350)
                .With(c => c.StuffingWeightKg, 25_000)
                .With(c => c.IncomingStocks, new List<IncomingStock>
                {
                    new() {Bags = 350, WeightKg = 25_000}
                })
                .Create();

            await new[] {container1, container2, container3, container4}.SaveList(session);
            WaitForIndexing(store);

            // Act
            var list = await sut.GetAvailableContainers(COMPANY_ID);

            // Assert
            list.Should().OnlyContain(c => c.Status == ContainerStatus.Empty || c.Status == ContainerStatus.Stuffing);
            list.Should().Contain(c => c.ContainerNumber == container1.ContainerNumber);
            list.Should().Contain(c => c.BookingNumber == container1.BookingNumber);
            list.Should().Contain(c => Math.Abs(c.Bags - container1.Bags) < 0.01);
            list.Should().Contain(c => Math.Abs(c.StockWeightKg - container1.StuffingWeightKg) < 0.01);
            list.Should().Contain(c => c.Id == container1.Id);

            var overweightContainer = list.Single(c => c.Id == container4.Id);
            overweightContainer.IsOverweight.Should().BeTrue();
        }

        [Fact]
        public async Task GetNonCommittedStocks()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockManagementService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var analysisResult = fixture.Build<AnalysisResult>().With(c => c.Approved, Approval.Approved).Create();

            var inspection = fixture.DefaultEntity<Inspection>()
                .With(c => c.AnalysisResult, analysisResult)
                .Create();
            await session.StoreAsync(inspection);

            var supplier1 = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier1);

            var supplier2 = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier2);


            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var stocks = fixture.DefaultEntity<Stock>()
                .With(c => c.IsStockIn, true)
                .With(c => c.InspectionId, inspection.Id)
                .With(c => c.LocationId, location.Id)
                .With(c => c.SupplierId, supplier1.Id)
                .CreateMany(10).ToList();

            stocks[9].SupplierId = supplier2.Id;
            await stocks.SaveList(session);

            var stockIds = stocks.Take(3).Select(c => c.Id).ToList();
            var purchaseDetail = fixture.Build<PurchaseDetail>()
                .With(c => c.StockIds, stockIds)
                .Create();

            var purchase = fixture.DefaultEntity<Purchase>()
                .With(c => c.PurchaseDetails, new List<PurchaseDetail> {purchaseDetail})
                .Create();

            await session.StoreAsync(purchase);
            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            var list1 = await sut.GetNonCommittedStocks(COMPANY_ID, supplier1.Id);
            var list2 = await sut.GetNonCommittedStocks(COMPANY_ID, supplier2.Id);

            // Assert
            list1.Should().HaveCount(6);
            list1.Should().BeInAscendingOrder(c => c.LotNo);
            list1.Select(x => x.StockId).Should().NotContain(stockIds);
            list1[0].SupplierName.Should().Be(supplier1.Name);
            list1[0].AnalysisResult.Approved.Should().Be(Approval.Approved);

            list2.Should().HaveCount(1);
            list2[0].SupplierName.Should().Be(supplier2.Name);
        }

        [Fact]
        public async Task MoveInspectionToStock_ShouldCreateANewStock()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockManagementService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            await session.StoreAsync(new Company(COMPANY_ID));

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var analysisResult = fixture.Build<AnalysisResult>()
                .With(c => c.Approved, Approval.Approved)
                .Create();

            var inspection = fixture.DefaultEntity<Inspection>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.Bags, 500)
                .With(c => c.AnalysisResult, analysisResult)
                .Without(c => c.StockReferences)
                .Create();
            await session.StoreAsync(inspection);
            await session.SaveChangesAsync();

            // Act
            const double bags = 400;

            var response = await sut.MoveInspectionToStock(inspection.Id, bags, new DateTime(2013, 1, 1), 0, location.Id);
            await session.SaveChangesAsync();

            // Assert

            // Should have created a stockIn
            response.Dto.StockId.Should().Be("stocks/1-A");
            var actualStock = await session.LoadAsync<Stock>(response.Dto.StockId);
            actualStock.StockInDate.Should().Be(new DateTime(2013, 1, 1));
            actualStock.SupplierId.Should().Be(supplier.Id);
            actualStock.Bags.Should().Be(400);
            actualStock.LocationId.Should().Be(location.Id);
            actualStock.LotNo.Should().Be(1);
            actualStock.InspectionId.Should().Be(inspection.Id);

            var listStocks = await session.Query<Stock>().ToListAsync();
            listStocks.Should().HaveCount(1);

            // Should have added the stockId to the inspection.StockIds
            var actualInspection = await session.LoadAsync<Inspection>(inspection.Id);
            actualInspection.StockReferences.Should().HaveCount(1);
            actualInspection.StockReferences[0].StockId.Should().Be(response.Dto.StockId);
            actualInspection.StockReferences[0].Bags.Should().Be(bags);
            actualInspection.StockReferences[0].Date.Should().Be(new DateTime(2013, 1, 1));
            actualInspection.StockReferences[0].LotNo.Should().Be(1);
        }

        [Fact]
        public async Task MoveInspectionToStock_ShouldCreateANewStockWithExistingLotNumber()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockManagementService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            await session.StoreAsync(new Company(COMPANY_ID));

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var analysisResult = fixture.Build<AnalysisResult>()
                .With(c => c.Approved, Approval.Approved)
                .Create();

            var inspection = fixture.DefaultEntity<Inspection>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.Bags, 500)
                .With(c => c.AnalysisResult, analysisResult)
                .Without(c => c.StockReferences)
                .Create();
            await session.StoreAsync(inspection);
            await session.SaveChangesAsync();

            // Act
            const double bags = 400;

            var response = await sut.MoveInspectionToStock(inspection.Id, bags, new DateTime(2013, 1, 1), 17, location.Id);
            await session.SaveChangesAsync();

            // Assert

            // Should have created a stockIn
            response.Dto.StockId.Should().Be("stocks/1-A");
            var actualStock = await session.LoadAsync<Stock>(response.Dto.StockId);
            actualStock.StockInDate.Should().Be(new DateTime(2013, 1, 1));
            actualStock.SupplierId.Should().Be(supplier.Id);
            actualStock.Bags.Should().Be(400);
            actualStock.LocationId.Should().Be(location.Id);
            actualStock.LotNo.Should().Be(17);
            actualStock.InspectionId.Should().Be(inspection.Id);

            var listStocks = await session.Query<Stock>().ToListAsync();
            listStocks.Should().HaveCount(1);

            // Should have added the stockId to the inspection.StockIds
            var actualInspection = await session.LoadAsync<Inspection>(inspection.Id);
            actualInspection.StockReferences.Should().HaveCount(1);
            actualInspection.StockReferences[0].StockId.Should().Be(response.Dto.StockId);
            actualInspection.StockReferences[0].Bags.Should().Be(bags);
            actualInspection.StockReferences[0].Date.Should().Be(new DateTime(2013, 1, 1));
            actualInspection.StockReferences[0].LotNo.Should().Be(17);
        }

        [Fact]
        public async Task RemoveInspectionFromStock_ShouldUpdateStockAndInspection()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockManagementService(session);
            var fixture = new Fixture();

            await session.StoreAsync(new Company(COMPANY_ID));

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var inspection = fixture.DefaultEntity<Inspection>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.Bags, 500)
                .With(c => c.StockReferences, new List<StockReference> {new("stocks/1-A", 400, new DateTime(2013, 1, 1), 1)})
                .Create();
            await session.StoreAsync(inspection);

            var stock = fixture.DefaultEntity<Stock>()
                .With(c => c.InspectionId, inspection.Id)
                .Create();
            await session.StoreAsync(stock);

            await session.SaveChangesAsync();

            // Act
            var response = await sut.RemoveInspectionFromStock(inspection.Id, stock.Id);

            // Assert
            response.Message.Should().Be("Removed inspection from stock");

            var actualInspection = await session.LoadAsync<Inspection>(inspection.Id);
            actualInspection.StockReferences.Should().HaveCount(0);

            var actualStock = await session.LoadAsync<Stock>(stock.Id);
            actualStock.InspectionId.Should().BeNullOrEmpty();
        }


        [Fact]
        public async Task StuffContainer_ShouldAddStockToTheContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockManagementService(session);
            var fixture = new Fixture();

            var stock1 = new Stock
            {
                CompanyId = COMPANY_ID,
                LotNo = 1,
                Bags = 3000,
                WeightKg = 24000,
                IsStockIn = true,
                StockInDate = new DateTime(1990, 12, 31),
                AnalysisResult = fixture.Build<AnalysisResult>().Create()
            };

            var stock2 = new Stock
            {
                CompanyId = COMPANY_ID,
                LotNo = 1,
                Bags = 500,
                WeightKg = 4000,
                IsStockIn = true,
                StockInDate = new DateTime(1992, 6, 30),
                AnalysisResult = fixture.Build<AnalysisResult>().Create()
            };
            await new[] {stock1, stock2}.SaveList(session);

            var stockBalance = new StockBalance
            {
                LotNo = 1,
                Balance = stock1.Bags + stock2.Bags,
                BalanceWeightKg = stock1.WeightKg + stock2.WeightKg,
                AnalysisResults = new List<AnalysisResult> {stock1.AnalysisResult, stock2.AnalysisResult},
                LocationId = "locations/1-A"
            };

            var container = new Container
            {
                ContainerNumber = "TRIU 1234"
            };
            await session.StoreAsync(container);

            await session.SaveChangesAsync();

            // Act
            const double incomingBags = 2000;
            const double incomingWeightKg = 16_000;
            ServerResponse response = await sut.StuffContainer(container.Id, stockBalance, incomingBags, incomingWeightKg, new DateTime(2020, 1, 1));

            await session.SaveChangesAsync();

            // Assert
            response.Message.Should().Be("Stuffed container");

            var actualContainer = await session.LoadAsync<Container>(container.Id);
            actualContainer.IncomingStocks.Should().HaveCount(1);
            actualContainer.IncomingStocks[0].LotNo.Should().Be(1);
            actualContainer.IncomingStocks[0].StockIds.Should().ContainEquivalentOf(new IncomingStockItem(stock1.Id, true));
            actualContainer.IncomingStocks[0].StockIds.Should().ContainEquivalentOf(new IncomingStockItem(stock2.Id, true));
            actualContainer.IncomingStocks[0].StockIds.Should().ContainEquivalentOf(new IncomingStockItem("stocks/3-A", false));
            actualContainer.IncomingStocks[0].Bags.Should().Be(incomingBags);
            actualContainer.IncomingStocks[0].WeightKg.Should().Be(incomingWeightKg);
            actualContainer.IncomingStocks[0].StuffingDate.Should().Be(new DateTime(2020, 1, 1));

            var actualStock1 = await session.LoadAsync<Stock>(stock1.Id);
            actualStock1.StuffingRecords[0].ContainerId.Should().Be(container.Id);
            actualStock1.StuffingRecords[0].ContainerNumber.Should().Be(container.ContainerNumber);
            actualStock1.StuffingRecords[0].StuffingDate.Should().Be(new DateTime(2020, 1, 1));
        }

        [Fact]
        public async Task StuffContainer_ShouldCreateStockOuts()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockManagementService(session);
            var fixture = new Fixture();

            var stock1 = new Stock
            {
                CompanyId = COMPANY_ID,
                LotNo = 1,
                Bags = 3000,
                WeightKg = 24000,
                IsStockIn = true,
                StockInDate = new DateTime(1990, 12, 31),
                AnalysisResult = fixture.Build<AnalysisResult>().With(c => c.Approved, Approval.Approved).Create(),
                Origin = "Origin1"
            };

            var stock2 = new Stock
            {
                CompanyId = COMPANY_ID,
                LotNo = 1,
                Bags = 500,
                WeightKg = 4000,
                IsStockIn = true,
                StockInDate = new DateTime(1992, 6, 30),
                AnalysisResult = fixture.Build<AnalysisResult>().With(c => c.Approved, Approval.Approved).Create(),
                Origin = "Origin2"
            };
            var stocks = new[] {stock1, stock2};
            await stocks.SaveList(session);

            var averageAnalysisResult = stocks.AverageAnalysisResults();

            var stockBalance = new StockBalance
            {
                LotNo = 1,
                Balance = stock1.Bags + stock2.Bags,
                BalanceWeightKg = stock1.WeightKg + stock2.WeightKg,
                AnalysisResults = new List<AnalysisResult> {stock1.AnalysisResult, stock2.AnalysisResult},
                LocationId = "locations/1-A",
                SupplierName = "Suppler Name",
                SupplierId = stock1.SupplierId
            };

            var container = new Container
            {
                ContainerNumber = "TRIU 1234"
            };
            await session.StoreAsync(container);

            await session.SaveChangesAsync();

            // Act
            const double incomingBags = 2000;
            const double incomingWeightKg = 16_000;
            var stuffingDate = new DateTime(2020, 1, 1);
            await sut.StuffContainer(container.Id, stockBalance, incomingBags, incomingWeightKg, stuffingDate);

            await session.SaveChangesAsync();

            // Assert
            var stockOut = await session.Query<Stock>().Where(c => !c.IsStockIn).FirstOrDefaultAsync();
            var actualContainer = await session.LoadAsync<Container>(container.Id);

            stockOut.Should().NotBeNull();
            stockOut.LotNo.Should().Be(1);
            stockOut.Bags.Should().Be(incomingBags);
            stockOut.WeightKg.Should().Be(incomingWeightKg);
            stockOut.AnalysisResult.Should().BeEquivalentTo(averageAnalysisResult);
            stockOut.LocationId.Should().Be(stockBalance.LocationId);
            stockOut.IsStockIn.Should().BeFalse();
            stockOut.SupplierId.Should().Be(stock1.SupplierId);
            stockOut.StockOutDate.Should().Be(stuffingDate);
            stockOut.StockInDate.Should().BeNull();
            stockOut.Origin.Should().Be("Origin1, Origin2");
            stockOut.CompanyId.Should().Be(COMPANY_ID);
            stockOut.StuffingRecords.Should().HaveCount(1);
            stockOut.StuffingRecords[0].ContainerId.Should().Be(container.Id);
            stockOut.StuffingRecords[0].ContainerNumber.Should().Be(container.ContainerNumber);
            stockOut.StuffingRecords[0].StuffingDate.Should().Be(stuffingDate);

            var incomingStocksIds = actualContainer.IncomingStocks.SelectMany(c => c.StockIds).Select(x=>x.StockId).ToList();
            incomingStocksIds.Should().Contain(new[] {stock1.Id, stock2.Id, stockOut.Id});
        }
    }
}