using System;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
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
                .With(c => c.StuffingDate, new DateTime(2013, 1, 1))
                .With(c => c.DispatchDate, default(DateTime?))
                .Without(c => c.Bags)
                .Without(c => c.NettWeightKg)
                .Create();

            // Act
            var response = await sut.Save(container);

            // Assert
            var actual = await session.LoadAsync<Container>(response.Id);
            actual.Should().NotBeNull();
            actual.StockWeightKg.Should().Be(container.IncomingStocks.Sum(c => c.WeightKg));
            actual.Bags.Should().Be(container.IncomingStocks.Sum(c => c.Bags));
        }
    }
}