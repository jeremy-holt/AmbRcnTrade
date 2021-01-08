using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.StockModels;
using AmbRcnTradeServer.Models.VesselModels;
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
            list.Should().BeInAscendingOrder(c => c.Status);
            list[0].Id.Should().Be(containers[1].Id);
            list[0].BookingNumber.Should().Be(containers[1].BookingNumber);
            list[0].NettWeightKg.Should().Be(containers[1].NettWeightKg);
            list[0].StuffingDate.Should().Be(containers[1].IncomingStocks[0].StuffingDate);
            list[0].IncomingStocks[0].Kor.Should().Be(containers[1].IncomingStocks[0].Kor);
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
                .With(c => c.VgmTicketNumber, "1234")
                .With(c => c.Teu, Teu.Teu40)
                .Create();

            // Act
            var response = await sut.Save(container);

            // Assert
            var actual = await session.LoadAsync<Container>(response.Id);
            actual.Should().NotBeNull();
            actual.StuffingWeightKg.Should().Be(container.IncomingStocks.Sum(c => c.WeightKg));
            actual.Bags.Should().Be(container.IncomingStocks.Sum(c => c.Bags));
            actual.WeighbridgeWeightKg.Should().Be(container.WeighbridgeWeightKg);
            actual.Teu.Should().Be(Teu.Teu40);
        }


        [Fact]
        public async Task UnStuffContainer_ShouldRemoveItFromTheContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            const string stockId1 = "stocks/1-A";
            const string stockId2 = "stocks/2-A";
            const string stockOutId = "stocks/3-A";

            var incomingStocks = new List<IncomingStock>
            {
                new()
                {
                    Bags = 302,
                    WeightKg = 24000,
                    LotNo = 21,
                    StockIds = new List<IncomingStockItem>
                    {
                        new(stockId1, true),
                        new(stockId2, true),
                        new(stockOutId, false)
                    }
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
                .With(c => c.Id, stockId1)
                .With(c => c.Bags, 500)
                .With(c => c.WeightKg, 8000)
                .Without(c => c.InspectionId)
                .With(c => c.IsStockIn, true)
                .With(c => c.StuffingRecords, stuffingRecords)
                .Create();
            await session.StoreAsync(stockIn1);

            var stockIn2 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockId2)
                .With(c => c.Bags, 50)
                .With(c => c.WeightKg, 1000)
                .Without(c => c.InspectionId)
                .With(c => c.IsStockIn, true)
                .With(c => c.StuffingRecords, stuffingRecords)
                .Create();
            await session.StoreAsync(stockIn2);

            var stockOut = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockOutId)
                .With(c => c.Bags, 110)
                .With(c => c.WeightKg, 3_0000)
                .With(c => c.IsStockIn, false)
                .With(c => c.StuffingRecords, stuffingRecords)
                .Create();

            await session.StoreAsync(stockOut);

            await session.SaveChangesAsync();

            // Act
            var response = await sut.UnStuffContainer(container.Id);
            await session.SaveChangesAsync();

            var actualStock1 = await session.LoadAsync<Stock>(stockId1);
            var actualStock2 = await session.LoadAsync<Stock>(stockId2);
            var actualStockOut = await session.LoadAsync<Stock>(stockOutId);
            var actualContainer = await session.LoadAsync<Container>(container.Id);

            // Assert
            actualStock1.Should().NotBeNull();
            actualStock1.StuffingRecords.Should().NotContain(c => c.ContainerId == container.Id);

            actualStock2.Should().NotBeNull();
            actualStock2.StuffingRecords.Should().NotContain(c => c.ContainerId == container.Id);

            actualStockOut.Should().BeNull();

            response.Message.Should().Be("Unstuffed container");

            actualContainer.Bags.Should().Be(0);
            actualContainer.StuffingWeightKg.Should().Be(0);
            actualContainer.IncomingStocks.Should().HaveCount(0);
        }


        [Fact]
        public async Task UnStuffContainer_ShouldThrowExceptionIfStatusIsNotEmptyOrStuffing()
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
            Func<Task> act = async () => await sut.UnStuffContainer(container.Id);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot remove stock from a container that is no longer in the warehouse");
        }

        [Fact]
        public async Task DeleteContainer_ShouldDeleteAAContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>()
                .Without(c=>c.IncomingStocks)
                .CreateMany().ToList();
            await containers.SaveList(session);
            
            var billLading = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, containers.GetPropertyFromList(c=>c.Id))
                .Create();
            await session.StoreAsync(billLading);

            await session.SaveChangesAsync();
            WaitForIndexing(store);
            
            // Act
            ServerResponse response = await sut.DeleteContainer(containers[1].Id);
            
            // Assert
            response.Message.Should().Be("Deleted container");
            var actualContainer = await session.LoadAsync<Container>(containers[1].Id);
            actualContainer.Should().BeNull();

            var actualBillLading = await session.LoadAsync<BillLading>(billLading.Id);
            actualBillLading.ContainerIds.Should().NotContain(containers[1].Id);
        }

        [Fact]
        public async Task DeleteContainer_ShouldDeleteANonBoardedContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();
            
            var containers = fixture.DefaultEntity<Container>()
                .Without(c=>c.IncomingStocks)
                .CreateMany().ToList();
            await containers.SaveList(session);
            
            var billLading = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, containers.GetPropertyFromList(c=>c.Id))
                .Create();
            billLading.ContainerIds.RemoveAll(c => c == containers[0].Id);
            await session.StoreAsync(billLading);
            
            // Act
            await sut.DeleteContainer(containers[0].Id);
            
            // Assert
            var actual = await session.LoadAsync<Container>(containers[0].Id);
            actual.Should().BeNull();
        }

        [Fact]
        public async Task DeleteContainer_ShouldThrowExceptionIfHasAlreadyBeenStuffed()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();
            
            var containers = fixture.DefaultEntity<Container>()
                .CreateMany().ToList();
            await containers.SaveList(session);
            
            var billLading = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, containers.GetPropertyFromList(c=>c.Id))
                .Create();
            await session.StoreAsync(billLading);

            await session.SaveChangesAsync();
            WaitForIndexing(store);
            
            // Act
            Func<Task> action = async ()=> await sut.DeleteContainer(containers[1].Id);
            
            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete a container that has already been stuffed");
        }
    }
}