using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.VesselModels;
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
    public class DraftBillLadingServiceTests : TestBaseContainer
    {
        public DraftBillLadingServiceTests(ITestOutputHelper output) : base(output) { }
        private const string MaerskDraftBlTemplateBlXlsx = "Maersk Draft BL Template BL.xlsx";

        [Fact]
        public async Task FillTemplate_ShouldFillTheTemplate()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetDraftBillLadingService(session);
            var fixture = new Fixture();

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var shipper = await new Customer().CreateAndStore(session);
            shipper.Address.City = "London";
            shipper.Address.State = null;
            shipper.Address.Country = "UK";
            var consignee = await new Customer().CreateAndStore(session);
            var notifyParty1 = await new Customer().CreateAndStore(session);
            var notifyParty2 = await new Customer().CreateAndStore(session);
            var forwardingAgent = await new Customer().CreateAndStore(session);
            var portDestination = await new Port().CreateAndStore(session);

            var billLading = await new BillLading().CreateAndStore(session);
            billLading.ContainerIds = containers.GetPropertyFromList(c => c.Id);
            billLading.ShipperId = shipper.Id;
            billLading.ConsigneeId = consignee.Id;
            billLading.NotifyParty1Id = notifyParty1.Id;
            billLading.NotifyParty2Id = notifyParty2.Id;
            billLading.PortOfDestinationId = portDestination.Id;
            billLading.DestinationAgentId = null;

            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, new List<string> {billLading.Id})
                .With(c => c.ForwardingAgentId, forwardingAgent.Id)
                .Create();
            await session.StoreAsync(vessel);
            await session.SaveChangesAsync();

            var response = await sut.LoadData(vessel.Id, billLading.Id);

            // Act
            var filledWorkbook = sut.FillTemplate(MaerskDraftBlTemplateBlXlsx, response);

            // Assert
            var savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Test Draft Bl.xlsx");
            filledWorkbook.Save(savePath);
        }

        [Fact]
        public async Task LoadData_ShouldNotThrowErrorIfBillLadingHasNoContainers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetDraftBillLadingService(session);
            var fixture = new Fixture();
            
            var billLading = await new BillLading().CreateAndStore(session);
            billLading.ContainerIds = new List<string>();
            
            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, new List<string> {billLading.Id})
                .Create();
            await session.StoreAsync(vessel);
            await session.SaveChangesAsync();
            
            // Act
            var data = await sut.LoadData(vessel.Id, billLading.Id);
            var actual =  sut.FillTemplate(MaerskDraftBlTemplateBlXlsx, data);
            
            // Assert
            data.BillLadingDto.Containers.Should().HaveCount(0);
        }

        [Fact]
        public async Task GetBillLadingCustomers_ShouldReturnBillLadingFormatCustomer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetDraftBillLadingService(session);
            var fixture = new Fixture();

            var shipper = await new Customer().CreateAndStore(session);
            shipper.Address.City = "London";
            shipper.Address.State = null;
            shipper.Address.Country = "UK";

            var consignee = await new Customer().CreateAndStore(session);
            var notifyParty1 = await new Customer().CreateAndStore(session);
            var notifyParty2 = await new Customer().CreateAndStore(session);
            var forwardingAgent = await new Customer().CreateAndStore(session);
            var portDestination = await new Port().CreateAndStore(session);

            var billLading = await new BillLading().CreateAndStore(session);
            billLading.ShipperId = shipper.Id;
            billLading.ConsigneeId = consignee.Id;
            billLading.NotifyParty1Id = notifyParty1.Id;
            billLading.NotifyParty2Id = notifyParty2.Id;
            billLading.PortOfDestinationId = portDestination.Id;
            billLading.DestinationAgentId = null;

            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, new List<string> {billLading.Id})
                .With(c => c.ForwardingAgentId, forwardingAgent.Id)
                .Create();
            await session.StoreAsync(vessel);
            await session.SaveChangesAsync();

            // Act
            var data = await sut.LoadData(vessel.Id, billLading.Id);
            var actual = sut.GetBillLadingCustomers(data);

            // Assert
            actual.Shipper.CompanyName.Value.Should().Be(shipper.CompanyName.ToUpper());
            actual.Shipper.Address1.Value.Should().Be(shipper.Address.Street1.ToUpper());
            actual.Shipper.Address2.Value.Should().Be(shipper.Address.Street2.ToUpper());
            actual.Shipper.Address3.Value.Should().Be("London UK".ToUpper());
            actual.Shipper.Address4.Value.Should().Be(shipper.Reference.ToUpper());
            actual.Shipper.Address5.Value.Should().Be(shipper.Email.ToUpper());

            actual.Shipper.CompanyName.Key.Should().Be("BillLading.ShipperCompanyName");
            actual.Shipper.Address1.Key.Should().Be("BillLading.ShipperAddress1");

            actual.DestinationAgent.CompanyName.Value.Should().Be("");
        }

        [Fact]
        public async Task GetBillLadingCustomers_ShouldReturn_EmptyLinesForMissingAddress()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetDraftBillLadingService(session);
            var fixture = new Fixture();

            var shipper = await new Customer().CreateAndStore(session);
            shipper.Address.Street1 = null;
            shipper.Address.Street2 = null;
            shipper.Address.City = null;
            shipper.Address.State = null;
            shipper.Address.Country = "UK";
            shipper.Reference = null;
            shipper.Email = null;

            var billLading = await new BillLading().CreateAndStore(session);
            billLading.ShipperId = shipper.Id;
            billLading.DestinationAgentId = null;

            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, new List<string> {billLading.Id})
                .Without(c => c.ForwardingAgentId)
                .Create();
            await session.StoreAsync(vessel);
            await session.SaveChangesAsync();

            // Act
            var data = await sut.LoadData(vessel.Id, billLading.Id);
            var actual = sut.GetBillLadingCustomers(data);

            // Assert
            actual.Shipper.CompanyName.Value.Should().Be(shipper.CompanyName.ToUpper());
            actual.Shipper.Address1.Value.Should().Be(shipper.Address.Country.ToUpper());
            actual.Shipper.Address2.Value.Should().Be("");
            actual.Shipper.Address3.Value.Should().Be("");
            actual.Shipper.Address4.Value.Should().Be("");
            actual.Shipper.Address5.Value.Should().Be("");

            actual.Shipper.CompanyName.Key.Should().Be("BillLading.ShipperCompanyName");
            actual.Shipper.Address1.Key.Should().Be("BillLading.ShipperAddress1");

            actual.DestinationAgent.Address1.Key.Should().Be("BillLading.DestinationAgentAddress1");
            actual.DestinationAgent.Address1.Value.Should().Be("");
        }

        [Fact]
        public async Task GetBillLadingCustomers_ShouldReturn_EmptyLinesForNullCustomer()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetDraftBillLadingService(session);
            var fixture = new Fixture();

            var billLading = await new BillLading().CreateAndStore(session);
            billLading.ShipperId = null;
            
            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, new List<string> {billLading.Id})
                .Without(c => c.ForwardingAgentId)
                .Create();
            await session.StoreAsync(vessel);
            await session.SaveChangesAsync();

            // Act
            var data = await sut.LoadData(vessel.Id, billLading.Id);
            var actual = sut.GetBillLadingCustomers(data);

            // Assert
            actual.Shipper.CompanyName.Value.Should().Be("");
            actual.Shipper.Address1.Value.Should().Be("");
            actual.Shipper.Address2.Value.Should().Be("");
            actual.Shipper.Address3.Value.Should().Be("");
            actual.Shipper.Address4.Value.Should().Be("");
            actual.Shipper.Address5.Value.Should().Be("");

            actual.Shipper.CompanyName.Key.Should().Be("BillLading.ShipperCompanyName");
            actual.Shipper.Address1.Key.Should().Be("BillLading.ShipperAddress1");

            actual.DestinationAgent.CompanyName.Key.Should().Be("BillLading.DestinationAgentCompanyName");
            actual.DestinationAgent.CompanyName.Value.Should().Be("");
        }

        [Fact]
        public async Task LoadData_ShouldLoadTheVesselAndBillOfLadingAndCustomers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetDraftBillLadingService(session);
            var fixture = new Fixture();

            var customers = fixture.DefaultEntity<Customer>().CreateMany().ToList();
            await customers.SaveList(session);

            var containers = fixture.DefaultEntity<Container>().CreateMany().ToList();
            await containers.SaveList(session);

            var billLading = await new BillLading().CreateAndStore(session);
            billLading.ContainerIds = containers.GetPropertyFromList(c => c.Id);
            billLading.ShipperId = customers[0].Id;

            var vessel = fixture.DefaultEntity<Vessel>()
                .With(c => c.BillLadingIds, new List<string> {billLading.Id})
                .Create();
            await session.StoreAsync(vessel);
            await session.SaveChangesAsync();

            // Act
            var response = await sut.LoadData(vessel.Id, billLading.Id);

            // Assert
            response.Vessel.Should().BeEquivalentTo(vessel);
            response.BillLadingDto.Containers[0].Bags.Should().NotBe(0);
        }

        [Fact]
        public void LoadTemplate_ShouldLoadTheDraftBlTemplate()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = (DraftBillLadingService) GetDraftBillLadingService(session);

            // Act
            var workbook = sut.LoadTemplate(MaerskDraftBlTemplateBlXlsx);

            // Assert
            workbook.Should().NotBeNull();
        }
    }
}