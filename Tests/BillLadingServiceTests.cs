using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.DraftBillLadingModels;
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
            await new Containers_Available_ForBillLading().ExecuteAsync(store);
            await new BillsOfLading_ByCustomers().ExecuteAsync(store);
            await new Vessels_ByBillLadingId().ExecuteAsync(store);
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
                .With(c => c.VesselId, "vessels/1-A")
                .Without(c => c.ContainerIds)
                .Create();
            await session.StoreAsync(billLading);

            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, billLading.ContainerIds)
                .Create();
            await session.StoreAsync(vessel);

            // Act
            var response = await sut.AddContainersToBillLading(billLading.Id, billLading.VesselId, containers.Select(x => x.Id).ToList());
            await session.SaveChangesAsync();
            WaitForIndexing(store);

            var actual = await session.LoadAsync<BillLading>(billLading.Id);

            // Assert
            response.Message.Should().Be("Added container(s) to Bill of Lading");
            actual.ContainerIds.Should().Contain(containers.Select(x => x.Id).ToList());

            using var session2 = store.OpenAsyncSession();
            var actualContainers = await session2.Query<Container>().ToListAsync();
            actualContainers.Should().OnlyContain(c => c.Status == ContainerStatus.OnBoardVessel);
            actualContainers.Should().OnlyContain(c => c.VesselId == vessel.Id);
        }

        [Fact]
        public async Task DeleteBillOfLading_ShouldDeleteBillAndReleaseContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>()
                .With(c => c.Status, ContainerStatus.OnBoardVessel)
                .CreateMany().ToList();
            await containers.SaveList(session);

            var billLading = fixture.DefaultEntity<BillLading>()
                .With(c => c.ContainerIds, containers.GetPropertyFromList(c => c.Id))
                .Create();
            await session.StoreAsync(billLading);

            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, new List<string> {billLading.Id})
                .Create();
            await session.StoreAsync(vessel);

            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            var response = await sut.DeleteBillLading(vessel.Id, billLading.Id);
            await session.SaveChangesAsync();

            // Assert
            response.Message.Should().Be("Deleted Bill of Lading");

            var actualVessel = await session.LoadAsync<Vessel>(vessel.Id);
            actualVessel.BillLadingIds.Should().NotContain(billLading.Id);

            var actualBillLading = await session.LoadAsync<BillLading>(billLading.Id);
            actualBillLading.Should().BeNull();

            var actualContainers = await session.Query<Container>().ToListAsync();
            actualContainers.Should().OnlyContain(c => c.Status == ContainerStatus.Gated);
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

            containers[0].Status = ContainerStatus.Gated;
            containers[1].Status = ContainerStatus.Gated;
            containers[2].Status = ContainerStatus.Gated;
            containers[3].Status = ContainerStatus.Stuffing;
            containers[4].Status = ContainerStatus.Empty;
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
            list.Should().NotContain(c => c.Id == containers[0].Id);
            list.Should().NotContain(c => c.Id == containers[4].Id);
            list.Should().NotContain(c => c.Status == ContainerStatus.OnBoardVessel);
            list.Should().NotContain(c => c.Status == ContainerStatus.Cancelled);
            list.Should().Contain(c => c.Status == ContainerStatus.Gated);
            list.Should().NotContain(c => c.Status == ContainerStatus.Empty);
            list.Should().Contain(c => c.StuffingWeightKg > 0);
        }

        [Fact]
        public void GetPreCargoDescription_ShouldReturnTextToInsertInCargoDescription()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);

            const double numberBags = 1280;
            const double numberContainers = 4;
            const double grossWeightKg = 101_790;
            const string productDescription = "Ivory Coast Origin 2021 season";
            const Teu teu = Teu.Teu40;
            const string declarationNumber = "DECL0001";
            // Act
            var actual = sut.GetPreCargoDescription(numberBags, numberContainers, grossWeightKg, productDescription, teu, declarationNumber);

            var expectedBodyText =
                $"{numberContainers}X40HC CONTAINER(S) SAID TO CONTAIN:\n" +
                $"{numberBags} JUTE BAGS OF DRIED RAW CASHEW NUTS IN SHELL\n" +
                "OF IVORY COAST ORIGIN - 2021 NEW CROP\n" +
                "HS CODE: 08013100";

            var expectedWeightsText = $"IN {numberBags} JUTE BAGS\n" +
                                      $"GROSS WEIGHT: {grossWeightKg:N0} KGS\n" +
                                      $"LESS WEIGHT OF EMPTY BAGS: {numberBags} KGS\n" +
                                      $"NET WEIGHT: {grossWeightKg - numberBags:N0} KGS\n" +
                                      "FREIGHT PREPAID\n" +
                                      $"DECLARATION NO: {declarationNumber}\n" +
                                      "21 FREE DAYS AT PORT OF DESTINATION";


            // Assert
            actual.Header.Should().Be(expectedBodyText);
            actual.Footer.Should().Be(expectedWeightsText);
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
                NotifyParty2Id = "customers/4-A"
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
        public async Task Load_ShouldCreateNewDocumentsIfTheDoNotAlreadyExist()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();

            var vessel = fixture.DefaultEntity<Vessel>()
                .Without(c => c.BillLadingIds)
                .Create();
            await session.StoreAsync(vessel);

            var blading = fixture.DefaultEntity<BillLadingDto>()
                .With(c => c.VesselId, vessel.Id)
                .Without(c => c.Documents)
                .Create();
            await session.StoreAsync(blading);

            // Act
            var actual = await sut.Load(blading.Id);

            // Assert
            actual.Documents.Should().HaveCount(6);
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
                .With(c => c.VesselId, "Vessels/1-A")
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
            actual.VesselId.Should().Be("Vessels/1-A");
        }

        [Fact]
        public async Task MoveBillLadingToVessel_ShouldMoveTheBillOfLadingToAnotherVessel()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();

            var billLading = await new BillLading().CreateAndStore(session);
            billLading.ContainersOnBoard = 0;
            billLading.ContainerIds = new List<string> {"containers/1-A", "containers/2-A"};
            await session.StoreAsync(billLading);

            var vessels = fixture.DefaultEntity<Vessel>()
                .Without(c => c.BillLadingIds)
                .Without(c => c.ContainersOnBoard)
                .CreateMany()
                .ToList();
            vessels[0].BillLadingIds.Add(billLading.Id);

            await vessels.SaveList(session);

            // Act
            var response = await sut.MoveBillLadingToVessel(billLading.Id, vessels[0].Id, vessels[1].Id);
            await session.SaveChangesAsync();

            // Assert
            response.Message.Should().Be("Moved Bill of Lading");

            var actualFirstVessel = await session.LoadAsync<Vessel>(vessels[0].Id);
            actualFirstVessel.BillLadingIds.Should().NotContain(billLading.Id);
            actualFirstVessel.ContainersOnBoard.Should().Be(0);

            var actualMovedToVessel = await session.LoadAsync<Vessel>(vessels[1].Id);
            actualMovedToVessel.BillLadingIds.Should().Contain(billLading.Id);
            actualMovedToVessel.ContainersOnBoard.Should().Be(2);
        }

        [Fact]
        public async Task RemoveContainersFromBillLading_ShouldRemoveTheContainersAndChangeStatusToGated()
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

            var actualContainer = await session.LoadAsync<Container>(containers[0].Id);
            actualContainer.Status.Should().Be(ContainerStatus.Gated);
            actualContainer.VesselId.Should().BeNullOrEmpty();
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

            var vessel = fixture.DefaultEntity<Vessel>()
                .Without(c => c.BillLadingIds)
                .Create();
            await session.StoreAsync(vessel);

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
                VesselId = vessel.Id,
                OwnReferences = "own references",
                ShipperReference = "shipper reference",
                ConsigneeReference = "consignee reference",
                DestinationAgentId = "customers/2-A",
                ShippingMarks = "shipping marks",
                ForwarderReference = "forwarder reference",
                PortOfDestinationId = "ports/1-A",
                PortOfLoadingId = "ports/2-A",
                OceanFreight = "570",
                FreightOriginCharges = "100",
                FreightOriginChargesPaidBy = "Shipper",
                FreightDestinationCharge = "Consignee",
                FreightDestinationChargePaidBy = "Abidjan/Dubai",
                OceanFreightPaidBy = "Notify",
                PortOfDestinationName = "HCM",
                ProductDescription = "IVC RCN",
                PreCargoDescription = new CargoDescription()
            };

            // Act
            var response = await sut.Save(billLadingDto);
            var actualVessel = await session.LoadAsync<Vessel>(vessel.Id);

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
            actual.VesselId.Should().Be(vessel.Id);
            actual.PortOfLoadingId.Should().Be("ports/2-A");
            actual.PortOfDestinationId.Should().Be("ports/1-A");

            actual.NumberBags.Should().Be(billLadingDto.Containers.Sum(x => x.Bags));
            actual.NettWeightKg.Should().Be(billLadingDto.Containers.Sum(x => x.NettWeightKg));
            actual.GrossWeightKg.Should().Be(billLadingDto.Containers.Sum(x => x.WeighbridgeWeightKg));

            actual.NumberPackagesText.Should().Be($"{actual.NumberBags} PACKAGES");
            actual.NettWeightKgText.Should().Be($"{actual.NettWeightKg:N0} KGS");
            actual.GrossWeightKgText.Should().Be($"{actual.GrossWeightKg:N0} KGS");
            actual.VgmWeightKgText.Should().Be(actual.GrossWeightKgText);
            actual.PreCargoDescription.Header.Should().NotBeNullOrEmpty();
            actual.PreCargoDescription.Footer.Should().NotBeNullOrEmpty();

            actualVessel.BillLadingIds.Should().Contain(response.Id);
        }

        [Fact]
        public async Task CreateBillOfLading_ShouldAddInitialDocumentsCheckList()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBillLadingService(session);
            var fixture = new Fixture();

            var vessel = fixture.DefaultEntity<Vessel>()
                .Without(c => c.BillLadingIds)
                .Create();
            await session.StoreAsync(vessel);

            var blading = fixture.DefaultEntity<BillLadingDto>()
                .With(c => c.VesselId, vessel.Id)
                .Without(c => c.Containers)
                .Without(c => c.Documents)
                .Create();

            // Act
            await sut.Save(blading);

            // Assert
            blading.Documents.Should().HaveCount(6);
        }
    }
}