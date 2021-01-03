using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
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
    public class ContainerServiceTests : TestBaseContainer
    {
        public ContainerServiceTests(ITestOutputHelper output) : base(output) { }


        [Fact]
        public async Task Load_ShouldLoadContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var container = fixture.DefaultEntity<Container>().Create();
            await session.StoreAsync(container);

            // Act
            var actual = await sut.Load(container.Id);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadList_ShouldLoadContainersBasedOnStatus()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>()
                .Without(c => c.Status)
                .CreateMany().ToList();

            containers[0].Status = ContainerStatus.Stuffing;
            containers[1].Status = ContainerStatus.OnBoardVessel;
            containers[2].Status = ContainerStatus.OnWayToPort;

            await containers.SaveList(session);

            // Act
            var list = await sut.LoadList(COMPANY_ID, ContainerStatus.OnBoardVessel);

            // Assert
            list.Should().HaveCount(1);
            list[0].Id.Should().Be(containers[1].Id);
            list[0].BookingNumber.Should().Be(containers[1].BookingNumber);
            list[0].NettWeightKg.Should().Be(containers[1].NettWeightKg);
        }

        [Fact]
        public async Task Save_ShouldSaveAContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var container = fixture.DefaultEntity<Container>()
                .With(c => c.DispatchDate, default(DateTime?))
                .Without(c => c.Bags)
                .Without(c => c.NettWeightKg)
                .Create();

            // Act
            var response = await sut.Save(container);

            // Assert
            var actual = await session.LoadAsync<Container>(response.Id);
            actual.Should().NotBeNull();
            actual.StuffingWeightKg.Should().Be(container.IncomingStocks.Sum(c => c.WeightKg));
            actual.Bags.Should().Be(container.IncomingStocks.Sum(c => c.Bags));
            actual.WeighbridgeWeightKg.Should().Be(container.WeighbridgeWeightKg);
        }


        [Fact]
        public async Task UnstuffContainer_ShouldThrowExceptionIfStatusIsNotEmptyOrStuffing()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var stock = fixture.DefaultEntity<Stock>().Create();
            await session.StoreAsync(stock);
            
            var container = fixture.DefaultEntity<Container>()
                .With(c => c.Status, ContainerStatus.OnWayToPort)
                .Create();
            await session.StoreAsync(container);

            // Act
            Func<Task> act = async () => await sut.UnstuffContainer(container.Id, stock.Id);
            
            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot remove stock from a container that is no longer in the warehouse");
        }
        
        
        [Fact]
        public async Task UnstuffContainer_ShouldRemoveItFromTheContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            const string stockId = "stocks/1-A";

            var incomingStocks = new List<IncomingStock>
            {
                new()
                {
                    Bags = 302,
                    WeightKg = 24000,
                    LotNo = 21,
                    StockIds = new List<string> {"stocks/33-A", stockId}
                }
            };

            var container = fixture.DefaultEntity<Container>()
                .With(c => c.Bags, 302)
                .With(c => c.StuffingWeightKg, 24000)
                .With(c => c.IncomingStocks, incomingStocks)
                .Create();
            await session.StoreAsync(container);

            var stuffingRecords = new List<StuffingRecord>
            {
                new() {ContainerId = container.Id, ContainerNumber = container.ContainerNumber}
            };
            
            var stockIn1 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockId)
                .With(c => c.Bags, 100)
                .With(c => c.WeightKg, 8000)
                .Without(c => c.InspectionId)
                .With(c => c.IsStockIn, true)
                .With(c => c.StuffingRecords,stuffingRecords)
                .Create();
            await session.StoreAsync(stockIn1);
            
            var stockIn2 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockId)
                .With(c => c.Bags, 50)
                .With(c => c.WeightKg, 1000)
                .Without(c => c.InspectionId)
                .With(c => c.IsStockIn, true)
                .With(c => c.StuffingRecords,stuffingRecords)
                .Create();
            await session.StoreAsync(stockIn2);

            await session.SaveChangesAsync();

            // Act
            ServerResponse response = await sut.UnstuffContainer(container.Id, stockId);
            await session.SaveChangesAsync();
            
            var actualStock = await session.LoadAsync<Stock>(stockId);
            var actualContainer = await session.LoadAsync<Container>(container.Id);

            // Assert
            actualStock.Should().NotBeNull();
            actualStock.StuffingRecords.Should().NotContain(c => c.ContainerId == container.Id);
            
            response.Message.Should().Be("Removed stock from container");
            
            actualContainer.Bags.Should().Be(302 - 100);
            actualContainer.StuffingWeightKg.Should().Be(24_000 - stockIn2.WeightKg);
            actualContainer.IncomingStocks[0].Bags.Should().Be(302 - 100);
            actualContainer.IncomingStocks[0].WeightKg.Should().Be(24_000 - stockIn2.WeightKg);
            actualContainer.IncomingStocks[0].StockIds.Should().NotContain(stockId);
        }
    }
}