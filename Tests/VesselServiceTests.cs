using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
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
    public class VesselServiceTests : TestBaseContainer
    {
        public VesselServiceTests(ITestOutputHelper output) : base(output) { }

        private async Task InitializeIndexes(IDocumentStore store)
        {
            await new Containers_ByVessel().ExecuteAsync(store);
        }

        [Fact]
        public async Task AddContainerToVessel_ShouldAddTheContainer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var vessel = fixture.DefaultEntity<Vessel>()
                .Without(c => c.ContainerIds)
                .Create();
            await session.StoreAsync(vessel);

            // Act
            var response = await sut.AddContainerToVessel(vessel.Id, containers.Select(x => x.Id).ToList());
            var actual = await session.LoadAsync<Vessel>(vessel.Id);

            // Assert
            response.Message.Should().Be("Added containers");
            actual.ContainerIds.Should().Contain(containers.Select(x => x.Id).ToList());
        }

        [Fact]
        public async Task GetNotLoadedContainer_ShouldReturnContainersThatAreNotOnBoardVessel()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetVesselService(session);
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

            var vessel1 = fixture.DefaultEntity<Vessel>()
                .With(c => c.ContainerIds, new List<string> {containers[0].Id})
                .Create();
            await session.StoreAsync(vessel1);

            var vessel2 = fixture.DefaultEntity<Vessel>()
                .With(c => c.ContainerIds, new List<string> {containers[4].Id})
                .Create();
            await session.StoreAsync(vessel2);

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
        public async Task Load_ShouldLoadVesselWithContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var vessel = new Vessel
            {
                CompanyId = COMPANY_ID,
                EtaHistory = new List<EtaHistory>
                {
                    new("Lollipop", new DateTime(2013, 1, 1))
                },
                ShippingCompany = "Maersk",
                ForwardingAgent = "Bollore",
                BlDate = default,
                BlNumber = "MER 1231",
                ContainerIds = containers.Select(x => x.Id).ToList(),
                ContainersOnBoard = 0
            };

            await session.StoreAsync(vessel);

            // Act
            var actual = await sut.Load(vessel.Id);

            // Assert
            for (var i = 0; i < 3; i++)
            {
                actual.Containers[i].Should().BeEquivalentTo(containers[i]);
            }
        }

        [Fact]
        public async Task LoadList_ShouldLoadListOfVessels()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var vessels = fixture.DefaultEntity<Vessel>().CreateMany().ToList();
            await vessels.SaveList(session);

            // Act
            var list = await sut.LoadList(COMPANY_ID);

            // Assert
            var expectedVessel = vessels.First(c => c.Id == list[0].Id);
            list.Should().BeInAscendingOrder(c => c.BlDate);
            var actual = list[0];

            var lastEta = expectedVessel.EtaHistory.FirstOrDefault(c => c.DateUpdated == expectedVessel.EtaHistory.Max(x => x.DateUpdated));
            actual.Eta.Should().Be(lastEta?.Eta);
            actual.VesselName.Should().Be(lastEta?.VesselName);
            actual.ContainersOnBoard.Should().Be(expectedVessel.ContainersOnBoard);
            actual.ForwardingAgent.Should().Be(expectedVessel.ForwardingAgent);
            actual.ShippingCompany.Should().Be(expectedVessel.ShippingCompany);
        }

        [Fact]
        public async Task RemoveContainersFromVessel_ShouldRemoveTheContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();


            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.ContainerIds, containers.Select(x => x.Id).ToList)
                .Create();
            await session.StoreAsync(vessel);

            // Act
            var response = await sut.RemoveContainersFromVessel(vessel.Id, new[] {containers[0].Id});
            await session.SaveChangesAsync();


            using var session2 = store.OpenAsyncSession();
            var actual = await session2.LoadAsync<Vessel>(vessel.Id);

            // Assert
            response.Message.Should().Be("Removed containers");
            actual.ContainerIds.Should().NotContain(containers[0].Id);

            response.Dto.Containers.Should().NotContain(c => c.Id == containers[0].Id);
            response.Dto.ContainerIds.Should().NotContain(containers[0].Id);
        }

        [Fact]
        public async Task Save_ShouldSaveAVessel()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var vesselDto = new VesselDto
            {
                CompanyId = COMPANY_ID,
                EtaHistory = new List<EtaHistory>
                {
                    new("Lollipop", new DateTime(2013, 1, 1))
                },
                ShippingCompany = "Maersk",
                ForwardingAgent = "Bollore",
                BlDate = default,
                BlNumber = "MER 1231",
                ContainerIds = containers.Select(x => x.Id).ToList(),
                ContainersOnBoard = 0,
                Containers = containers,
                NotifyParty1="notify 1",
                NotifyParty2="notify 2",
                Consignee="consignee",
                BlBodyText="body text",
                FreightPrepaid=true
            };

            // Act
            var response = await sut.Save(vesselDto);

            // Assert
            var actual = await session.LoadAsync<Vessel>(response.Id);
            actual.ContainerIds.Should().BeEquivalentTo(containers.Select(x => x.Id));
            actual.EtaHistory.Should().BeEquivalentTo(vesselDto.EtaHistory);
            actual.ContainersOnBoard.Should().Be(3);
            actual.BlDate.Should().Be(vesselDto.BlDate);
            actual.BlNumber.Should().Be(vesselDto.BlNumber);
            actual.CompanyId.Should().Be(vesselDto.CompanyId);
            actual.ForwardingAgent.Should().Be(vesselDto.ForwardingAgent);
            actual.ShippingCompany.Should().Be(vesselDto.ShippingCompany);
            actual.NotifyParty1.Should().Be("notify 1");
            actual.NotifyParty2.Should().Be("notify 2");
            actual.Consignee.Should().Be("consignee");
            actual.BlBodyText.Should().Be("body text");
            actual.FreightPrepaid.Should().BeTrue();
        }
    }
}