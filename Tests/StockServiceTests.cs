using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Models;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.StockModels;
using AutoFixture;
using FluentAssertions;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tests.Base;
using Xunit;
using Xunit.Abstractions;
using Inspection = AmbRcnTradeServer.Models.InspectionModels.Inspection;

namespace Tests
{
    public class StockServiceTests : TestBaseContainer
    {
        public StockServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Load_ShouldLoadStock()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();

            var stock = fixture.DefaultEntity<Stock>().Create();
            await session.StoreAsync(stock);

            // Act
            var actual = await sut.Load(stock.Id);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task Save_ShouldGenerateLotNumber()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();

            await session.StoreAsync(new Company {Id = COMPANY_ID});
            await session.SaveChangesAsync();

            var stockIn = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
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
            var fixture = new Fixture();

            var stockOut = fixture.DefaultEntity<Stock>()
                .With(c => c.StockOutDate, new DateTime(2013, 1, 1))
                .With(c => c.Bags, 100)
                .With(c => c.WeightKg, 80_000)
                .Without(c => c.StockInDate)
                .Create();

            // Act
            var response = await sut.Save(stockOut);

            // Assert
            var actual = await sut.Load(response.Id);
            actual.Bags.Should().Be(-100);
            actual.WeightKg.Should().Be(-80_000);
        }

        [Fact]
        public async Task Save_ShouldNotGenerateLotNumberForStockOut()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var fixture = new Fixture();

            var stockOut = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockInDate)
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
            var fixture = new Fixture();

            var stock = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
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
            var fixture = new Fixture();

            await session.StoreAsync(new Company(COMPANY_ID));

            var location = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(location);

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
                InspectionIds = new List<string>()
            };

            // Act
            var response = await sut.Save(stockIn);

            // Assert
            var actual = await session.LoadAsync<Stock>(response.Id);
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
        public async Task Load_ShouldUpdateAverageKor()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetStockService(session);
            var inspectionService = GetInspectionService(session);

            var fixture = new Fixture();

            var analyses = fixture.Build<Analysis>()
                .With(c => c.Kor, 50)
                .With(c => c.Bags, 100)
                .CreateMany().ToList();
            var inspections = fixture.DefaultEntity<Inspection>()
                .With(c => c.Analyses, analyses)
                .With(c => c.Bags, 100)
                .CreateMany()
                .ToList();

            foreach (var inspection in inspections)
                await inspectionService.Save(inspection);

            var expectedAvgKor = inspections.Sum(x => x.AnalysisResult.Kor * x.Bags) / inspections.Sum(x => x.Bags);

            await session.SaveChangesAsync();

            var stockIn = fixture.DefaultEntity<Stock>()
                .Without(c => c.StockOutDate)
                .Without(c => c.AnalysisResult)
                .With(c => c.InspectionIds, inspections.Select(x => x.Id).ToList)
                .Create();
            await session.StoreAsync(stockIn);

            // Act
            var actual = await sut.Load(stockIn.Id);

            // Assert
            actual.AnalysisResult.Kor.Should().Be(expectedAvgKor);
        }
    }
}