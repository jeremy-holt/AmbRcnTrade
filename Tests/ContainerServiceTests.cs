﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.DictionaryModels;
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
            var response = await sut.DeleteContainer(containers[1].Id);

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


        [Fact]
        public async Task Load_ShouldLoadContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var vessel = await new Vessel().CreateAndStoreAsync(session);

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
        public async Task LoadList_ShouldLoadVesselNameAndWarehouseName()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var vessel = await new Vessel().CreateAndStoreAsync(session);

            var warehouse = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(warehouse);

            var containers = fixture.DefaultEntity<Container>()
                .With(c => c.VesselId, vessel.Id)
                .With(c => c.WarehouseId, warehouse.Id)
                .With(c => c.PackingListId, "packingLists/1-A")
                .With(c => c.TareKg, 3900)
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
            actual.WarehouseId.Should().Be(warehouse.Id);
            actual.WarehouseName.Should().Be(warehouse.Name);
            actual.PackingListId.Should().Be("packingLists/1-A");
            actual.TareKg.Should().Be(3900);
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
                .With(c => c.ExporterSealNumber, "Mangro")
                .With(c => c.Teu, Teu.Teu40)
                .With(c => c.WarehouseId, "customers/1-A")
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
            actual.WarehouseId.Should().Be("customers/1-A");
        }

        [Fact]
        public async Task Save_ShouldUpdateNettWeight()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);
            var fixture = new Fixture();

            var container = fixture.DefaultEntity<Container>()
                .With(c => c.WeighbridgeWeightKg, 10000)
                .With(c => c.TareKg, 500)
                .Without(c => c.NettWeightKg)
                .Create();

            // Act
            await sut.Save(container);

            // Assert
            var actual = await session.LoadAsync<Container>(container.Id);
            actual.NettWeightKg.Should().Be(10_000 - 500);
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
                new() {ContainerId = container2.Id, ContainerNumber = container2.ContainerNumber}
            };

            var stuffingRecords2 = new List<StuffingRecord>
            {
                new() {ContainerId = container3.Id, ContainerNumber = container3.ContainerNumber}
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
                .With(c => c.StuffingRecords, new List<StuffingRecord> {new() {ContainerId = container1.Id, ContainerNumber = container1.ContainerNumber}})
                .Create();

            var stockOut2 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockOut2Id)
                .With(c => c.Bags, 70)
                .With(c => c.WeightKg, 5600)
                .With(c => c.IsStockIn, false)
                .With(c => c.StuffingRecords, new List<StuffingRecord> {new() {ContainerId = container2.Id, ContainerNumber = container2.ContainerNumber}})
                .Create();

            var stockOut3 = fixture.DefaultEntity<Stock>()
                .With(c => c.Id, stockOut3Id)
                .With(c => c.Bags, 50)
                .With(c => c.WeightKg, 400)
                .With(c => c.IsStockIn, false)
                .With(c => c.StuffingRecords, new List<StuffingRecord> {new() {ContainerId = container3.Id, ContainerNumber = container3.ContainerNumber}})
                .Create();

            var stocks = new[] {stockIn1, stockIn2, stockOut1, stockOut2, stockOut3};
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
        public async Task UnstuffContainer_ShouldUnstuffRealData()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetContainerService(session);

            var container = new Container
            {
                Id = "containers/242-A",
                ContainerNumber = "PONU 1927533",
                Bags = 345,
                WeighbridgeWeightKg = 27950,
                Status = ContainerStatus.StuffingComplete,
                StuffingWeightKg = 31886,
                IncomingStocks = new List<IncomingStock>
                {
                    new()
                    {
                        Bags = 18, WeightKg = 1539.75, LotNo = 46, StuffingDate = new DateTime(2021, 4, 14), Kor = 47.26,
                        StockIds = new List<IncomingStockItem>
                        {
                            new() {StockId = "stocks/338-A", IsStockIn = true},
                            new() {StockId = "stocks/574-A", IsStockIn = false}
                        }
                    },
                    new()
                    {
                        Bags = 204, WeightKg = 19251, LotNo = 47, StuffingDate = new DateTime(2021, 4, 14), Kor = 45,
                        StockIds = new List<IncomingStockItem>
                        {
                            new() {StockId = "stocks/339-A", IsStockIn = true},
                            new() {StockId = "stocks/575-A", IsStockIn = false}
                        }
                    },
                    new()
                    {
                        Bags = 123, WeightKg = 11096, LotNo = 49, StuffingDate = new DateTime(2021, 4, 14), Kor = 47.26,
                        StockIds = new List<IncomingStockItem>
                        {
                            new() {StockId = "stocks/354-A", IsStockIn = true},
                            new() {StockId = "stocks/576-A", IsStockIn = false}
                        }
                    }
                },
                NettWeightKg = 24140,
                StuffingDate = new DateTime(2014, 4, 14),
                CompanyId = "companies/1-A",
                WarehouseId = "customers/4-A",
                WarehouseName = "BOUAKE",
                TareKg = 3810
            };

            await session.StoreAsync(container);

            var stocks = await "ContainerServiceData-Stocks.json".JsonFileToClassAsync<List<Stock>>();
            await stocks.SaveList(session);
            await session.SaveChangesAsync();

            WaitForUserToContinueTheTest(store);

            // Act
            await sut.UnStuffContainer(container.Id);

            var stock1 = await session.LoadAsync<Stock>("stocks/338-A"); // Stock In
            var stock2 = await session.LoadAsync<Stock>("stocks/574-A"); // Stock Out
            var stock3 = await session.LoadAsync<Stock>("stocks/339-A"); // Stock In
            var stock4 = await session.LoadAsync<Stock>("stocks/575-A"); // Stock Out
            var stock5 = await session.LoadAsync<Stock>("stocks/354-A"); // Stock In
            var stock6 = await session.LoadAsync<Stock>("stocks/576-A"); // Stock Out

            var actualStocksIn = new List<Stock> {stock1, stock3, stock5};

            stock2.Should().BeNull();
            stock4.Should().BeNull();
            stock6.Should().BeNull();

            var actualContainer = await session.LoadAsync<Container>(container.Id);


            // Assert
            foreach (var stock in actualStocksIn)
            {
                stock.StuffingRecords.Should().NotContain(c => c.ContainerId == container.Id);
                stock.StuffingRecords.Should().NotContain(c => c.ContainerNumber == container.ContainerNumber);
                stock.StuffingRecords.Should().NotContain(c => c.StuffingDate == container.StuffingDate);
            }

            stock1.Bags.Should().Be(480);
            stock1.WeightKg.Should().Be(41060);
            stock1.LotNo.Should().Be(46);
            stock1.IsStockIn.Should().BeTrue();

            actualContainer.Bags.Should().Be(0);
            actualContainer.StuffingWeightKg.Should().Be(0);
            actualContainer.IncomingStocks.Should().HaveCount(0);
            actualContainer.Status.Should().Be(ContainerStatus.Empty);
        }
    }
}