using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.PackingListModels;
using AmbRcnTradeServer.Services;
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
    public class PackingListServiceTests : TestBaseContainer
    {
        public PackingListServiceTests(ITestOutputHelper output) : base(output) { }

        private IPackingListService GetPackingListService(IAsyncDocumentSession session)
        {
            return new PackingListService(session);
        }

        [Fact]
        public async Task Load_ShouldLoadContainersWithPackingList()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var packingList = fixture.DefaultEntity<PackingList>()
                .With(c => c.ContainerIds, containers.Select(c => c.Id).ToList)
                .Without(c => c.Containers)
                .Create();

            await session.StoreAsync(packingList);
            await session.SaveChangesAsync();

            // Act
            var actual = await sut.Load(packingList.Id);

            // Assert
            actual.Containers.Should().Contain(c => c.Id == containers[0].Id);
        }

        [Fact]
        public async Task Load_ShouldLoadPackingList()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();

            // Act
            var packingList = fixture.DefaultEntity<PackingList>().Create();
            await session.StoreAsync(packingList);
            var actual = await sut.Load(packingList.Id);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task Save_ShouldSaveAPackingList()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();

            var packingList = fixture.DefaultEntity<PackingList>()
                .Without(c => c.ContainerIds)
                .With(c => c.BookingNumber, "Booking")
                .With(c => c.Date, new DateTime(2013, 1, 1))
                .With(c => c.Notes, "Notes")
                .Create();

            // Act
            var response = await sut.Save(packingList);

            // Assert
            response.Message.Should().Be("Saved");
            var actual = await session.LoadAsync<PackingList>(response.Id);
            actual.Date.Should().Be(new DateTime(2013, 1, 1));
            actual.Notes.Should().Be("Notes");
            actual.BookingNumber.Should().Be("Booking");
            actual.ContainerIds.Should().HaveCount(0);
        }

        [Fact]
        public async Task Save_ShouldUpdateContainerWithPackingListId()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>()
                .Without(c => c.PackingListId)
                .CreateMany().ToList();
            await containers.SaveList(session);

            await session.SaveChangesAsync();

            var packingList = fixture.DefaultEntity<PackingList>()
                .Without(c => c.Containers)
                .With(c => c.ContainerIds, containers.Select(c => c.Id).ToList)
                .Create();

            // Act
            await sut.Save(packingList);
            await session.SaveChangesAsync();

            // Assert
            var actual = await session.Query<Container>().ToListAsync();
            actual[0].PackingListId.Should().Be(packingList.Id);
        }

        [Fact]
        public async Task Save_ShouldClearPackingListIdFromAnyUnusedContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();
            
            var containers = fixture.DefaultEntity<Container>()
                .Without(c => c.PackingListId)
                .CreateMany().ToList();
            await containers.SaveList(session);
            
            await session.SaveChangesAsync();

            var packingList = fixture.DefaultEntity<PackingList>()
                .Without(c => c.Containers)
                .With(c => c.ContainerIds, containers.Select(c => c.Id).ToList)
                .Create();

            await sut.Save(packingList);
            
            // Act
            var firstContainerId = packingList.ContainerIds[0];
            packingList.ContainerIds.RemoveAt(0);
            await sut.Save(packingList);

            var actualContainer = await session.LoadAsync<Container>(firstContainerId);
            actualContainer.PackingListId.Should().BeNullOrEmpty();

        }

        [Fact]
        public async Task GetNonAllocatedContainers_ShouldReturnContainersWithoutAPackingListId()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();
            
            var containers = fixture.DefaultEntity<Container>()
                .Without(c => c.PackingListId)
                .CreateMany().ToList();
            containers[0].PackingListId = "packingLists/1-A";
            
            await containers.SaveList(session);
            
            // Act
            List<Container> list = await sut.GetNonAllocatedContainers(COMPANY_ID);
            
            // Assert
            list.Should().HaveCount(2).And.OnlyContain(c => c.PackingListId.IsNullOrEmpty());
        }

        [Fact]
        public async Task RemoveContainerFromPackingList_ShouldRemoveTheContainerFromThePackingList()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();
            
            var containers = fixture.DefaultEntity<Container>()
                .Without(c => c.PackingListId)
                .CreateMany().ToList();
            await containers.SaveList(session);

            await session.SaveChangesAsync();

            var packingList = fixture.DefaultEntity<PackingList>()
                .Without(c => c.Containers)
                .With(c => c.ContainerIds, containers.Select(c => c.Id).ToList)
                .Create();
            
            await sut.Save(packingList);
            await session.SaveChangesAsync();
            
            // Act

            var containerId = containers[0].Id;
            var response = await sut.RemoveContainerFromPackingList(containerId, packingList.Id);
            
            // Assert
            response.Message.Should().Be("Removed container");
            response.Dto.ContainerIds.Should().NotContain(containerId);

            var actualContainer = await session.LoadAsync<Container>(containerId);
            actualContainer.PackingListId.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task LoadList_ShouldLoadListOfPackingLists()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();
            
            var containers = fixture.DefaultEntity<Container>()
                .Without(c => c.PackingListId)
                .CreateMany().ToList();
            await containers.SaveList(session);

            await session.SaveChangesAsync();

            var packingList = fixture.DefaultEntity<PackingList>()
                .Without(c => c.Containers)
                .With(c => c.ContainerIds, containers.Select(c => c.Id).ToList)
                .Create();
            
            await sut.Save(packingList);
            await session.SaveChangesAsync();
            
            // Act
            List<PackingList> list = await sut.LoadList(COMPANY_ID);
            
            // Assert
            list.Should().HaveCount(1);
            list[0].Containers[0].Id.Should().Be(list[0].ContainerIds[0]);
            list.Should().BeInAscendingOrder(c => c.Id);
        }

        [Fact]
        public async Task DeletePackingList_ShouldRemoveItsReferenceFromAllContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetPackingListService(session);
            var fixture = new Fixture();
            
            var containers = fixture.DefaultEntity<Container>()
                .Without(c => c.PackingListId)
                .CreateMany().ToList();
            await containers.SaveList(session);

            await session.SaveChangesAsync();

            var packingList = fixture.DefaultEntity<PackingList>()
                .Without(c => c.Containers)
                .With(c => c.ContainerIds, containers.Select(c => c.Id).ToList)
                .Create();
            
            await sut.Save(packingList);
            await session.SaveChangesAsync();
            
            // Act
            ServerResponse response = await sut.Delete(packingList.Id);
            
            // Assert
            response.Message.Should().Be("Deleted packing list");
            var actualPackingList = await session.LoadAsync<PackingList>(packingList.Id);
            actualPackingList.Should().BeNull();

            var actualContainers = await session.Query<Container>().ToListAsync();
            actualContainers.Should().OnlyContain(c => c.PackingListId.IsNullOrEmpty());
        }
    }
}