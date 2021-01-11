using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
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
    public class StockServiceTests : TestBaseContainer
    {
        public StockServiceTests(ITestOutputHelper output) : base(output) { }

        private static async Task InitializeIndexes(IDocumentStore store)
        {
            await new Stocks_ByBalances().ExecuteAsync(store);
            await new Stocks_ById().ExecuteAsync(store);
            await new Inspections_ByAnalysisResult().ExecuteAsync(store);
        }

        [Fact]
        public async Task Delete_StockIn_ShouldDeleteStockAndRemoveItFromInspection()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();

            const string inspectionId = "inspections/1-A";
            const string stockId = "stocks/1-A";
            const double weightKg = 29_999;

            var stockReferences = new List<StockReference>
            {
                new(stockId, 100, weightKg, DateTime.Today, 1),
                new("stocks/2-A", 100, weightKg, DateTime.Today, 1)
            };

            var inspection = fixture.DefaultEntity<Inspection>()
                .With(c => c.Id, inspectionId)
                .With(c => c.StockReferences, stockReferences)
                .Create();

            await session.StoreAsync(inspection);

            var stock = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockId)
                .With(c => c.InspectionId, inspectionId)
                .With(c => c.IsStockIn, true)
                .Without(c => c.StuffingRecords)
                .Create();
            await session.StoreAsync(stock);

            await session.SaveChangesAsync();

            // Act
            var response = await sut.DeleteStock(stockId);
            await session.SaveChangesAsync();

            // Assert
            response.Message.Should().Be("Deleted stock");
            using var session2 = store.OpenAsyncSession();

            var actualStock = await session2.LoadAsync<Stock>(stockId);
            actualStock.Should().BeNull();

            var actualInspection = await session2.LoadAsync<Inspection>(inspectionId);
            actualInspection.StockReferences.Should().OnlyContain(c => c.StockId == "stocks/2-A");
            actualInspection.StockReferences.Should().Contain(c => Math.Abs(c.WeightKg - weightKg) < 0.01);
        }

        [Fact]
        public async Task Delete_StockIn_ShouldThrowExceptionIfStockIsAlreadyStuffedInContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();

            const string inspectionId = "inspections/1-A";
            const string stockId = "stocks/1-A";

            var stock = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockId)
                .With(c => c.InspectionId, inspectionId)
                .With(c => c.IsStockIn, true)
                .With(c => c.StuffingRecords, fixture.Build<StuffingRecord>().CreateMany().ToList)
                .Create();
            await session.StoreAsync(stock);

            await session.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await sut.DeleteStock(stockId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This stock has already been stuffed into a container. It cannot be deleted");
        }

        [Fact]
        public async Task Delete_StockOut_ShouldThrowException()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();

            const string inspectionId = "inspections/1-A";
            const string stockId = "stocks/1-A";

            var stock = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockId)
                .With(c => c.InspectionId, inspectionId)
                .With(c => c.IsStockIn, false)
                .With(c => c.StuffingRecords, fixture.Build<StuffingRecord>().CreateMany().ToList)
                .Create();
            await session.StoreAsync(stock);

            await session.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await sut.DeleteStock(stockId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This stock has already been stuffed into a container. It cannot be deleted");
        }

        [Fact]
        public async Task Load_ShouldLoadStock()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var inspectionService = GetInspectionService(session);

            await InitializeIndexes(store);
            var fixture = new Fixture();

            var analyses = new List<Analysis>
            {
                new() {Count = 200, Moisture = 7, SoundGm = 255, SpottedGm = 10}
            };
            var inspection = fixture.DefaultEntity<Inspection>()
                .With(c => c.Analyses, analyses)
                .Create();
            await inspectionService.Save(inspection);

            var stock = fixture.DefaultEntity<Stock>()
                .With(c => c.InspectionId, inspection.Id)
                .Without(c => c.AnalysisResult)
                .Create();
            await session.StoreAsync(stock);

            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            var actual = await sut.Load(stock.Id);

            // Assert
            actual.Should().NotBeNull();
            actual.AnalysisResult.Count.Should().Be(inspection.AnalysisResult.Count);
        }

        [Fact]
        public async Task LoadStockBalanceList_ShouldLoadStockMovementsForLotNo()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            await session.StoreAsync(new Company(COMPANY_ID));

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var supplier1 = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier1);

            var supplier2 = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier2);

            var stockIn1 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .With(c => c.LocationId, location.Id)
                .With(c => c.SupplierId, supplier1.Id)
                .With(c => c.LotNo, 1)
                .With(c => c.InspectionId, "inspections/1-A")
                .With(c => c.IsStockIn, true)
                .With(c => c.ZeroedStock, true)
                .Create();
            await session.StoreAsync(stockIn1);

            var stockIn2 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .With(c => c.LocationId, location.Id)
                .With(c => c.SupplierId, supplier2.Id)
                .With(c => c.LotNo, 2)
                .With(c => c.Bags, 500)
                .With(c => c.InspectionId, "inspections/1-A")
                .With(c => c.IsStockIn, true)
                .With(c => c.ZeroedStock, true)
                .Create();
            await session.StoreAsync(stockIn2);

            var stockOut = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockInDate)
                .With(c => c.LotNo, stockIn2.LotNo)
                .With(c => c.LocationId, location.Id)
                .With(c => c.SupplierId, supplier2.Id)
                .With(c => c.LotNo, 2)
                .With(c => c.Bags, 150)
                .With(c => c.InspectionId, "inspections/1-A")
                .With(c => c.IsStockIn, false)
                .Create();
            await session.StoreAsync(stockOut);

            var stockIn3 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .With(c => c.LocationId, location.Id)
                .With(c => c.SupplierId, supplier1.Id)
                .With(c => c.LotNo, 3)
                .With(c => c.InspectionId, "inspections/1-A")
                .With(c => c.IsStockIn, true)
                .Create();
            await session.StoreAsync(stockIn3);

            await session.SaveChangesAsync();

            WaitForIndexing(store);

            // Act
            var list = await sut.LoadStockBalanceList(COMPANY_ID, null);

            // Assert
            list.Should().HaveCount(3);
            list.Should().OnlyHaveUniqueItems(c => c.LotNo);

            var actual = list.First(c => c.LotNo == stockIn2.LotNo);
            actual.LotNo.Should().Be(2);
            actual.ZeroedStock.Should().BeTrue();

            var expectedBalanceBags = stockIn2.Bags - stockOut.Bags;
            var expectedBalanceStockWeightKg = stockIn2.WeightKg - stockOut.WeightKg;

            actual.BagsIn.Should().Be(stockIn2.Bags);
            actual.BagsOut.Should().Be(stockOut.Bags);
            actual.Balance.Should().Be(expectedBalanceBags);

            actual.BalanceWeightKg.Should().Be(expectedBalanceStockWeightKg);

            actual.LocationId.Should().Be(stockIn2.LocationId);
            actual.LocationName.Should().Be(location.Name);

            actual.SupplierId.Should().Be(stockIn2.SupplierId);
            actual.SupplierName.Should().Be(supplier2.Name);
            actual.AnalysisResults.Should().HaveCountGreaterThan(0);
            actual.AvgBagWeightKg.Should().Be(actual.BalanceWeightKg / actual.Balance);
        }

        [Fact]
        public async Task LoadStockList_ShouldLoadListOfStocks()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();
            await InitializeIndexes(store);

            await session.StoreAsync(new Company(COMPANY_ID));

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var analysisResult = fixture.Build<AnalysisResult>().With(c => c.Approved, Approval.Approved).Create();

            var inspections = fixture.DefaultEntity<Inspection>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.AnalysisResult, analysisResult)
                .CreateMany().ToList();
            await inspections.SaveList(session);

            var stockIn1 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .Without(c => c.InspectionId)
                .Without(c => c.LocationId)
                .With(c => c.ZeroedStock, true)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await sut.Save(stockIn1);

            var stockIn2 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .With(c => c.InspectionId, inspections[0].Id)
                .With(c => c.LocationId, location.Id)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await sut.Save(stockIn2);

            var stockOut = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockInDate)
                .Without(c => c.InspectionId)
                .With(c => c.LotNo, stockIn2.LotNo)
                .Without(c => c.LocationId)
                .Without(c => c.SupplierId)
                .Create();
            await sut.Save(stockOut);

            var stockIn3 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .Without(c => c.InspectionId)
                .Without(c => c.LocationId)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await sut.Save(stockIn3);

            await session.SaveChangesAsync();

            WaitForIndexing(store);

            // Act
            var list = await sut.LoadStockList(COMPANY_ID, (string) null);
            list.Should().BeInAscendingOrder(c => c.LotNo);

            var actual = list.Single(c => c.StockId == stockIn2.Id);
            actual.BagsIn.Should().Be(stockIn2.Bags);
            actual.BagsOut.Should().Be(0);
            actual.LocationId.Should().Be(stockIn2.LocationId);
            actual.LocationName.Should().Be(location.Name);
            actual.LotNo.Should().Be(stockIn2.LotNo);
            actual.IsStockIn.Should().BeTrue();
            actual.StockDate.Should().Be(stockIn2.StockInDate ?? DateTime.MinValue);
            actual.StockDate.Should().NotBe(DateTime.MinValue);
            actual.Origin.Should().Be(stockIn2.Origin);
            actual.SupplierName.Should().Be(supplier.Name);
            actual.SupplierId.Should().Be(supplier.Id);
            actual.InspectionId.Should().Be(stockIn2.InspectionId);
            actual.WeightKgIn.Should().Be(stockIn2.WeightKg);
            actual.AvgBagWeightKg.Should().Be(stockIn2.WeightKg / stockIn2.Bags);
        }

        [Fact]
        public async Task LoadStockList_ShouldLoadListOfStocksForStockIds()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();
            await InitializeIndexes(store);

            await session.StoreAsync(new Company(COMPANY_ID));

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var analysisResult = fixture.Build<AnalysisResult>().With(c => c.Approved, Approval.Approved).Create();

            var inspections = fixture.DefaultEntity<Inspection>()
                .With(c => c.SupplierId, supplier.Id)
                .With(c => c.AnalysisResult, analysisResult)
                .CreateMany().ToList();
            await inspections.SaveList(session);

            var stockIn1 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .Without(c => c.InspectionId)
                .Without(c => c.LocationId)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await sut.Save(stockIn1);

            var stockIn2 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .With(c => c.InspectionId, inspections[0].Id)
                .With(c => c.LocationId, location.Id)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await sut.Save(stockIn2);

            var stockOut = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockInDate)
                .Without(c => c.InspectionId)
                .With(c => c.LotNo, stockIn2.LotNo)
                .Without(c => c.LocationId)
                .Without(c => c.SupplierId)
                .Create();
            await sut.Save(stockOut);

            var stockIn3 = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .Without(c => c.InspectionId)
                .Without(c => c.LocationId)
                .With(c => c.SupplierId, supplier.Id)
                .Create();
            await sut.Save(stockIn3);

            await session.SaveChangesAsync();

            WaitForIndexing(store);

            // Act
            var list = await sut.LoadStockList(COMPANY_ID, new List<string> {stockIn1.Id, stockIn2.Id});
            list.Should().BeInAscendingOrder(c => c.LotNo);
            list.Should().HaveCount(2);
            list.Should().Contain(c => c.StockId == stockIn1.Id);
            list.Should().Contain(c => c.StockId == stockIn2.Id);
        }

        [Fact]
        public async Task Save_ShouldGenerateLotNumber()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            await session.StoreAsync(new Company {Id = COMPANY_ID});
            await session.SaveChangesAsync();

            var stockIn = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .Without(c => c.InspectionId)
                .Without(c => c.LotNo)
                .Create();

            // Act
            var response = await sut.Save(stockIn);

            // Assert
            var actual = await sut.Load(response.Id);
            actual.LotNo.Should().Be(1L);
        }

        [Fact]
        public async Task Save_ShouldMakeQuantitiesNegativeIfStockOut()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var stockOut = fixture.DefaultEntity<Stock>()
                .With(c => c.StockOutDate, new DateTime(2013, 1, 1))
                .Without(c => c.InspectionId)
                .With(c => c.Bags, 100)
                .With(c => c.WeightKg, 80_000)
                .Without(c => c.StockInDate)
                .Create();

            // Act
            var response = await sut.Save(stockOut);

            // Assert
            var actual = await sut.Load(response.Id);
            actual.Bags.Should().Be(100);
            actual.WeightKg.Should().Be(80_000);
        }

        [Fact]
        public async Task Save_ShouldNotGenerateLotNumberForStockOut()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var stockOut = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockInDate)
                .Without(c => c.InspectionId)
                .Without(c => c.LotNo)
                .Create();

            // Act
            var response = await sut.Save(stockOut);

            // Assert
            var actual = await sut.Load(response.Id);
            actual.LotNo.Should().Be(0);
        }

        [Fact]
        public async Task Save_ShouldNotOverwriteExistingLotNo()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var inspection = fixture.DefaultEntity<Inspection>().Create();
            await session.StoreAsync(inspection);

            var stock = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .With(c => c.InspectionId, inspection.Id)
                .With(c => c.Id, "stocks/1-A")
                .With(c => c.LotNo, 45)
                .Create();
            await session.StoreAsync(stock);

            var existingStock = await sut.Load(stock.Id);
            existingStock.Bags = 777;

            // Act
            await sut.Save(existingStock);

            // Assert
            var actual = await session.LoadAsync<Stock>(stock.Id);
            actual.Bags.Should().Be(777);
            actual.LotNo.Should().Be(45);
        }

        [Fact]
        public async Task Save_ShouldSaveStock()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            await session.StoreAsync(new Company(COMPANY_ID));

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

            var supplier = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(supplier);

            var stockIn = new Stock
            {
                Id = null,
                CompanyId = COMPANY_ID,
                LocationId = location.Id,
                StockInDate = new DateTime(2013, 1, 1),
                StockOutDate = default,
                LotNo = 1,
                Bags = 300.0,
                WeightKg = 24000.0,
                SupplierId = supplier.Id,
                InspectionId = null,
                Origin = "Bouake"
            };

            // Act
            var response = await sut.Save(stockIn);

            // Assert
            var actual = await session.LoadAsync<Stock>(response.Id);
            actual.IsStockIn.Should().BeTrue();
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task Save_ShouldThrowExceptionIfStockInDate_And_StockOutDateAreProvided()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();

            var stock = fixture.DefaultEntity<Stock>().Create();

            // Act
            Func<Task> action = async () => await sut.Save(stock);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("A stock cannot have both a Stock In date and a Stock Out date");
        }

        [Fact]
        public async Task ZeroStockBalance_ShouldManuallyMarkStockAndLotsAsPhysicallyEmpty()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();

            var stocksIn = fixture.DefaultEntity<Stock>()
                .With(c => c.IsStockIn, true)
                .With(c => c.LotNo, 3)
                .CreateMany().ToList();
            await stocksIn.SaveList(session);

            var stockOut = fixture.DefaultEntity<Stock>()
                .With(c => c.IsStockIn, false)
                .With(c => c.LotNo, 3)
                .Create();
            await session.StoreAsync(stockOut);

            // Act
            ServerResponse response = await sut.ZeroStock(COMPANY_ID, 3);
            var actualStocks = await session.Query<Stock>().ToListAsync();

            // Assert
            response.Message.Should().Be("Marked Lot 3 as having zero physical stock");
            actualStocks.Where(c => c.IsStockIn).Should().Contain(c => c.ZeroedStock);

            var actualStockOut = await session.LoadAsync<Stock>(stockOut.Id);
            actualStockOut.ZeroedStock.Should().BeFalse();
        }
    }
}