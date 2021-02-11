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

            var vessel = await new Vessel().CreateAndStore(session);

            var container = fixture.DefaultEntity<Container>()
                .With(c => c.VesselId, vessel.Id)
                .Without(c => c.VesselName)
                .Create();
            await session.StoreAsync(container);

            // Act
            var actual = await sut.Load(container.Id);

            // Assert
            actual.Should().NotBeNull();
            actual.VesselName.Should().Be($"{vessel.VesselName} {vessel.VoyageNumber}");
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
        public async Task LoadList_ShouldLoadVesselName()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var vessel = await new Vessel().CreateAndStore(session);

            var containers = fixture.DefaultEntity<Container>()
                .With(c => c.VesselId, vessel.Id)
                .Without(c => c.VesselName)
                .CreateMany()
                .ToList();
            await containers.SaveList(session);
            WaitForIndexing(store);

            // Act
            var list = await sut.LoadList(COMPANY_ID, null);

            // Assert
            var actual = list[0];
            actual.VesselName.Should().Be(vessel.VesselName + " " + vessel.VoyageNumber);
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
                .With(c=>c.ExporterSealNumber, "Mangro")
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
            actual.ExporterSealNumber.Should().Be("Mangro");
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
            const string stockOut1Id = "stocks/3-A";
            const string stockOut2Id = "stocks/4-A";
            const string stockOut3Id = "stocks/5-A";

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
                        new(stockOut1Id, false)
                    }
                }
            };

            var container1 = fixture.DefaultEntity<Container>()
                .With(c => c.Bags, 302)
                .With(c => c.StuffingWeightKg, 24000)
                .With(c => c.IncomingStocks, incomingStocks)
                .Create();
            await session.StoreAsync(container1);
            
            var container2 = fixture.DefaultEntity<Container>()
                .With(c => c.Bags, 100)
                .With(c => c.StuffingWeightKg, 8000)
                .With(c => c.IncomingStocks, incomingStocks)
                .Create();
            await session.StoreAsync(container2);
            
            var container3 = fixture.DefaultEntity<Container>()
                .With(c => c.Bags, 100)
                .With(c => c.StuffingWeightKg, 8000)
                .With(c => c.IncomingStocks, incomingStocks)
                .Create();
            await session.StoreAsync(container3);
            

            var stuffingRecords1 = new List<StuffingRecord>
            {
                new() {ContainerId = container1.Id, ContainerNumber = container1.ContainerNumber},
                new (){ContainerId = container2.Id,ContainerNumber = container2.ContainerNumber}
            };
            
            var stuffingRecords2 = new List<StuffingRecord>
            {
                new (){ContainerId = container3.Id,ContainerNumber = container3.ContainerNumber}
            };
            
            var stockIn1 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockId1)
                .With(c => c.Bags, 500)
                .With(c => c.WeightKg, 8000)
                .Without(c => c.InspectionId)
                .With(c => c.IsStockIn, true)
                .With(c => c.StuffingRecords, stuffingRecords1)
                .Create();
            
            var stockIn2 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockId2)
                .With(c => c.Bags, 50)
                .With(c => c.WeightKg, 1000)
                .Without(c => c.InspectionId)
                .With(c => c.IsStockIn, true)
                .With(c => c.StuffingRecords, stuffingRecords2)
                .Create();
            
            var stockOut1 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockOut1Id)
                .With(c => c.Bags, 110)
                .With(c => c.WeightKg, 3_0000)
                .With(c => c.IsStockIn, false)
                .With(c => c.StuffingRecords, new List<StuffingRecord>(){new(){ContainerId = container1.Id,ContainerNumber = container1.ContainerNumber}})
                .Create();
            
            var stockOut2 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockOut2Id)
                .With(c => c.Bags, 70)
                .With(c => c.WeightKg, 5600)
                .With(c => c.IsStockIn, false)
                .With(c => c.StuffingRecords, new List<StuffingRecord>(){new(){ContainerId = container2.Id,ContainerNumber = container2.ContainerNumber}})
                .Create();
            
            var stockOut3 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockOut3Id)
                .With(c => c.Bags, 50)
                .With(c => c.WeightKg, 400)
                .With(c => c.IsStockIn, false)
                .With(c => c.StuffingRecords, new List<StuffingRecord>(){new(){ContainerId = container3.Id, ContainerNumber = container3.ContainerNumber}})
                .Create();
            
            var stocks = new[] {stockIn1, stockIn2,stockOut1, stockOut2, stockOut3};
            await stocks.SaveList(session);
            

            // Act
            var response = await sut.UnStuffContainer(container1.Id);
            await session.SaveChangesAsync();

            var actualStock1 = await session.LoadAsync<Stock>(stockId1);
            var actualStock2 = await session.LoadAsync<Stock>(stockId2);
            
            var actualStockOut1 = await session.LoadAsync<Stock>(stockOut1Id);
            var actualStockOut2 = await session.LoadAsync<Stock>(stockOut2Id);
            var actualStockOut3 = await session.LoadAsync<Stock>(stockOut3Id);
            var actualContainer = await session.LoadAsync<Container>(container1.Id);

            // Assert
            actualStock1.Should().NotBeNull();
            actualStock1.StuffingRecords.Should().NotContain(c => c.ContainerId == container1.Id);

            actualStock2.Should().NotBeNull();
            actualStock2.StuffingRecords.Should().NotContain(c => c.ContainerId == container1.Id);

            actualStockOut1.Should().BeNull();
            actualStockOut2.Should().NotBeNull();
            actualStockOut3.Should().NotBeNull();

            response.Message.Should().Be("Unstuffed container");

            actualContainer.Bags.Should().Be(0);
            actualContainer.StuffingWeightKg.Should().Be(0);
            actualContainer.IncomingStocks.Should().HaveCount(0);
            actualContainer.Status.Should().Be(ContainerStatus.Empty);
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
                .Without(c => c.IncomingStocks)
                .CreateMany().ToList();
            await containers.SaveList(session);

            var billLading = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, containers.GetPropertyFromList(c => c.Id))
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
                .Without(c => c.IncomingStocks)
                .CreateMany().ToList();
            await containers.SaveList(session);

            var billLading = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, containers.GetPropertyFromList(c => c.Id))
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
                .With(c => c.ContainerIds, containers.GetPropertyFromList(c => c.Id))
                .Create();
            await session.StoreAsync(billLading);

            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            Func<Task> action = async () => await sut.DeleteContainer(containers[1].Id);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete a container that has already been stuffed");
        }
    }
}