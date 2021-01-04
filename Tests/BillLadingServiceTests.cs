using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.VesselModels;
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
    public class BillLadingServiceTests : TestBaseContainer
    {
        public BillLadingServiceTests(ITestOutputHelper output) : base(output) { }

        private static async Task InitializeIndexes(IDocumentStore store)
        {
            await new Containers_ByBillLading().ExecuteAsync(store);
            await new BillsOfLading_ByCustomers().ExecuteAsync(store);
        }

        [Fact]
        public async Task AddContainersToBillOfLading_ShouldAddTheContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var billLading = fixture.DefaultEntity<BillLading>()
                .Without(c => c.ContainerIds)
                .Create();
            await session.StoreAsync(billLading);

            // Act
            var response = await sut.AddContainersToBillLading(billLading.Id, containers.Select(x => x.Id).ToList());
            var actual = await session.LoadAsync<BillLading>(billLading.Id);

            // Assert
            response.Message.Should().Be("Added container(s) to Bill of Lading");
            actual.ContainerIds.Should().Contain(containers.Select(x => x.Id).ToList());
        }

        [Fact]
        public async Task GetNotLoadedContainers_ShouldReturnContainersThatAreNotOnBoardVessel()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();
            var containers = fixture.DefaultEntity<Container>()
                .With(c => c.Status, ContainerStatus.Empty)
                .CreateMany(10).ToList();

            containers[0].Status = ContainerStatus.OnBoardVessel;
            containers[1].Status = ContainerStatus.Gated;
            containers[2].Status = ContainerStatus.Gated;
            containers[3].Status = ContainerStatus.Stuffing;
            containers[4].Status = ContainerStatus.OnBoardVessel;
            containers[5].Status = ContainerStatus.OnBoardVessel;

            await containers.SaveList(session);

            var billLading1 = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, new List<string> {containers[0].Id})
                .Create();
            await session.StoreAsync(billLading1);

            var billLading2 = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, new List<string> {containers[4].Id})
                .Create();
            await session.StoreAsync(billLading2);

            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            var list = await sut.GetNotLoadedContainers(COMPANY_ID);

            // Assert
            list.Should().NotContain(c => c.ContainerId == containers[0].Id);
            list.Should().NotContain(c => c.Status == ContainerStatus.OnBoardVessel);
            list.Should().NotContain(c => c.Status == ContainerStatus.Cancelled);
            list.Should().Contain(c => c.Status == ContainerStatus.Empty || c.Status == ContainerStatus.Gated);
        }

        [Fact]
        public async Task Load_ShouldLoadBillLadingWithContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var billLading = new BillLading
            {
                CompanyId = COMPANY_ID,
                BlDate = default,
                BlNumber = "MER 1231",
                ContainerIds = containers.Select(x => x.Id).ToList(),
                ContainersOnBoard = 0,
                ConsigneeId = "customers/1-A",
                ShipperId = "customers/2-A",
                NotifyParty1Id = "customers/3-A",
                NotifyParty2Id = "customers/4-A",
                PortOfDestinationId = "ports/1-A"
            };

            await session.StoreAsync(billLading);

            // Act
            var actual = await sut.Load(billLading.Id);

            // Assert
            for (var i = 0; i < 3; i++)
            {
                actual.Containers[i].Should().BeEquivalentTo(containers[i]);
            }
        }

        [Fact]
        public async Task LoadList_ShouldLoadListOfBillsOfLading()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();

            var customers = fixture.DefaultEntity<Customer>().CreateMany(4).ToList();
            await customers.SaveList(session);

            var port = fixture.DefaultEntity<Port>().Create();
            await session.StoreAsync(port);

            var billOfLadings = fixture.DefaultEntity<BillLading>()
                .With(c => c.ConsigneeId, customers[0].Id)
                .With(c => c.NotifyParty1Id, customers[1].Id)
                .With(c => c.NotifyParty2Id, customers[2].Id)
                .With(c => c.ShipperId, customers[3].Id)
                .With(c => c.PortOfDestinationId, port.Id)
                .CreateMany().ToList();
            await billOfLadings.SaveList(session);
            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            var list = await sut.LoadList(COMPANY_ID);

            // Assert
            var expectedBillLading = billOfLadings.First(c => c.Id == list[0].Id);
            list.Should().BeInAscendingOrder(c => c.BlDate);
            var actual = list[0];

            actual.ContainersOnBoard.Should().Be(expectedBillLading.ContainersOnBoard);
            actual.ConsigneeName.Should().Be(customers[0].Name);
            actual.NotifyParty1Name.Should().Be(customers[1].Name);
            actual.NotifyParty2Name.Should().Be(customers[2].Name);
            actual.ShipperName.Should().Be(customers[3].Name);
            actual.PortOfDestinationName.Should().Be(port.Name);
        }

        [Fact]
        public async Task RemoveContainersFromBillLading_ShouldRemoveTheContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var billLading = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, containers.Select(x => x.Id).ToList)
                .Create();
            await session.StoreAsync(billLading);

            // Act
            var response = await sut.RemoveContainersFromBillLading(billLading.Id, new[] {containers[0].Id});
            await session.SaveChangesAsync();

            using var session2 = store.OpenAsyncSession();
            var actual = await session2.LoadAsync<BillLading>(billLading.Id);

            // Assert
            response.Message.Should().Be("Removed containers from Bill of Lading");
            actual.ContainerIds.Should().NotContain(containers[0].Id);

            response.Dto.Containers.Should().NotContain(c => c.Id == containers[0].Id);
            response.Dto.ContainerIds.Should().NotContain(containers[0].Id);
        }

        [Fact]
        public async Task Save_ShouldSaveABillLading()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var billLadingDto = new BillLadingDto
            {
                CompanyId = COMPANY_ID,
                BlDate = default,
                BlNumber = "MER 1231",
                ContainerIds = containers.Select(x => x.Id).ToList(),
                ContainersOnBoard = 0,
                Containers = containers,
                NotifyParty1Id = "notify 1",
                NotifyParty2Id = "notify 2",
                ConsigneeId = "consignee",
                ShipperId = "shipper",
                BlBodyText = "body text",
                FreightPrepaid = true,
                PortOfDestinationId = "ports/1-A"
            };

            // Act
            var response = await sut.Save(billLadingDto);

            // Assert
            var actual = await session.LoadAsync<BillLading>(response.Id);
            actual.ContainerIds.Should().BeEquivalentTo(containers.Select(x => x.Id));
            actual.ContainersOnBoard.Should().Be(3);
            actual.BlDate.Should().Be(billLadingDto.BlDate);
            actual.BlNumber.Should().Be(billLadingDto.BlNumber);
            actual.CompanyId.Should().Be(billLadingDto.CompanyId);
            actual.NotifyParty1Id.Should().Be("notify 1");
            actual.NotifyParty2Id.Should().Be("notify 2");
            actual.ConsigneeId.Should().Be("consignee");
            actual.ShipperId.Should().Be("shipper");
            actual.BlBodyText.Should().Be("body text");
            actual.FreightPrepaid.Should().BeTrue();
            actual.PortOfDestinationId.Should().Be("ports/1-A");
        }
    }
}