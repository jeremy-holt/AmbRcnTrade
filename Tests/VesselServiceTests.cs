﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
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
    public class VesselServiceTests : TestBaseContainer
    {
        public VesselServiceTests(ITestOutputHelper output) : base(output) { }


        [Fact]
        public async Task AddBillLadingToVessel_ShouldAddBillLadingToVessel()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var billLadings = fixture.DefaultEntity<BillLading>().CreateMany().ToList();
            await billLadings.SaveList(session);

            var vessel = fixture.DefaultEntity<Vessel>()
                .Without(c => c.BillLadingIds)
                .Create();
            await session.StoreAsync(vessel);

            // Act
            var response = await sut.AddBillLadingToVessel(vessel.Id, billLadings.Select(x => x.Id).ToList());
            var actual = await session.LoadAsync<Vessel>(vessel.Id);

            // Assert
            response.Message.Should().Be("Added Bill of Lading to vessel");
            actual.BillLadingIds.Should().Contain(billLadings.Select(x => x.Id).ToList());
        }


        [Fact]
        public async Task Load_ShouldLoadVesselWithBillsOfLading()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var billLadings = fixture.DefaultEntity<BillLading>().CreateMany().ToList();
            await billLadings.SaveList(session);

            var customers = fixture.DefaultEntity<Customer>().CreateMany(2).ToList();
            await customers.SaveList(session);

            var vessel = new Vessel
            {
                CompanyId = COMPANY_ID,
                EtaHistory = new List<EtaHistory>
                {
                    new("Lollipop", new DateTime(2013, 1, 1))
                },
                ShippingCompanyId = customers[0].Id,
                ForwardingAgentId = customers[1].Id,
                BillLadingIds = billLadings.Select(x => x.Id).ToList(),
                ContainersOnBoard = 0
            };

            await session.StoreAsync(vessel);

            // Act
            var actual = await sut.Load(vessel.Id);

            // Assert
            for (var i = 0; i < 3; i++)
            {
                actual.BillLadings[i].Should().BeEquivalentTo(billLadings[i]);
            }
        }

        [Fact]
        public async Task LoadList_ShouldLoadListOfVessels()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var port = fixture.DefaultEntity<Port>().Create();
            await session.StoreAsync(port);

            var customers = fixture.DefaultEntity<Customer>().CreateMany(2).ToList();
            await customers.SaveList(session);

            var vessels = fixture.DefaultEntity<Vessel>()
                .With(c => c.PortOfDestinationId, port.Id)
                .With(c => c.ShippingCompanyId, customers[0].Id)
                .With(c => c.ForwardingAgentId, customers[1].Id)
                .CreateMany().ToList();
            await vessels.SaveList(session);
            WaitForIndexing(store);

            // Act
            var list = await sut.LoadList(COMPANY_ID);

            // Assert
            var expectedVessel = vessels.First(c => c.Id == list[0].Id);
            list.Should().BeInAscendingOrder(c => c.Eta);
            var actual = list[0];

            var lastEta = expectedVessel.EtaHistory.FirstOrDefault(c => c.DateUpdated == expectedVessel.EtaHistory.Max(x => x.DateUpdated));
            actual.Eta.Should().Be(lastEta?.Eta);
            actual.VesselName.Should().Be(lastEta?.VesselName);
            actual.ContainersOnBoard.Should().Be(expectedVessel.ContainersOnBoard);
            actual.ShippingCompanyName.Should().Be(customers[0].Name);
            actual.ForwardingAgentName.Should().Be(customers[1].Name);
            actual.PortOfDestinationName.Should().Be(port.Name);
        }

        private async Task InitializeIndexes(IDocumentStore store)
        {
            await new Vessels_ByCustomers().ExecuteAsync(store);
        }

        [Fact]
        public async Task RemoveBillsLadingFromVessel_ShouldRemoveTheBillsLading()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var billsLading = fixture.DefaultEntity<BillLading>().CreateMany().ToList();
            await billsLading.SaveList(session);

            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, billsLading.Select(x => x.Id).ToList)
                .Create();
            await session.StoreAsync(vessel);

            // Act
            var response = await sut.RemoveBillsLadingFromVessel(vessel.Id, new[] {billsLading[0].Id});
            await session.SaveChangesAsync();

            using var session2 = store.OpenAsyncSession();
            var actual = await session2.LoadAsync<Vessel>(vessel.Id);

            // Assert
            response.Message.Should().Be("Removed Bills of Lading from vessel");
            actual.BillLadingIds.Should().NotContain(billsLading[0].Id);

            response.Dto.BillLadings.Should().NotContain(c => c.Id == billsLading[0].Id);
            response.Dto.BillLadingIds.Should().NotContain(billsLading[0].Id);
        }

        [Fact]
        public async Task Save_ShouldSaveAVessel()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetVesselService(session);
            var fixture = new Fixture();

            var billLadings = fixture.DefaultEntity<BillLading>().CreateMany().ToList();
            await billLadings.SaveList(session);

            var vesselDto = new VesselDto
            {
                CompanyId = COMPANY_ID,
                EtaHistory = new List<EtaHistory>
                {
                    new("Lollipop", new DateTime(2013, 1, 1))
                },
                ShippingCompanyId = "customers/1-A",
                ForwardingAgentId = "customers/2-A",
                BillLadingIds = billLadings.Select(x => x.Id).ToList(),
                ContainersOnBoard = 0,
                BillLadings = billLadings
            };

            // Act
            var response = await sut.Save(vesselDto);

            // Assert
            var actual = await session.LoadAsync<Vessel>(response.Id);
            actual.BillLadingIds.Should().BeEquivalentTo(billLadings.Select(x => x.Id));
            actual.EtaHistory.Should().BeEquivalentTo(vesselDto.EtaHistory);
            actual.ContainersOnBoard.Should().Be(9);
            actual.CompanyId.Should().Be(vesselDto.CompanyId);
            actual.ForwardingAgentId.Should().Be(vesselDto.ForwardingAgentId);
            actual.ShippingCompanyId.Should().Be(vesselDto.ShippingCompanyId);
        }
    }
}