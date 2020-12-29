using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
using AutoFixture;
using FluentAssertions;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Tests.Base;
using Xunit;
using Xunit.Abstractions;
using Inspection = AmbRcnTradeServer.Models.InspectionModels.Inspection;

namespace Tests
{
    public class InspectionServiceTests : TestBaseContainer
    {
        public InspectionServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task Load_ShouldLoadInspection()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var inspection = fixture.DefaultEntity<Inspection>().Create();
            await session.StoreAsync(inspection);

            // Act
            var actual = await sut.Load(inspection.Id);

            // Assert
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task LoadList_ShouldLoadListOfInspections()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var customer = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(customer);

            var analysisResult = fixture.Build<Analysis>()
                .With(c => c.Approved, Approval.Approved)
                .Create();

            var inspections = fixture.DefaultEntity<Inspection>()
                .With(c => c.AnalysisResult,analysisResult)
                .With(c => c.SupplierId, customer.Id)
                .CreateMany()
                .ToList();
            inspections[2].AnalysisResult.Approved = Approval.Rejected;

            await inspections.SaveList(session);
            await session.SaveChangesAsync();
            WaitForIndexing(store);

            var prms = new InspectionQueryParams
            {
                CompanyId = COMPANY_ID,
                Approved = Approval.Approved,
            };

            // Act
            var list = await sut.LoadList(prms);

            var actual = list.Single(c => c.Id == inspections[0].Id);
            var expected = inspections[0];

            // Assert
            list.Should().Contain(c => c.Approved == prms.Approved);
            list.Should().BeInDescendingOrder(c => c.InspectionDate);

            actual.LotNo.Should().Be(expected.LotNo);
            actual.Inspector.Should().Be(expected.Inspector);
            actual.Bags.Should().Be(expected.Bags);
            actual.Approved.Should().Be(expected.AnalysisResult.Approved);
            actual.Location.Should().Be(expected.Location);
            actual.TruckPlate.Should().Be(expected.TruckPlate);
            actual.SupplierName.Should().Be(customer.Name);
            actual.SupplierId.Should().Be(customer.Id);
            actual.Kor.Should().BeGreaterThan(0);
            actual.Count.Should().BeGreaterThan(0);
            actual.Moisture.Should().BeGreaterThan(0);
            actual.RejectsPct.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Save_ShouldSaveAnInspection()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);

            var analyses = new List<Analysis>
            {
                new()
                {
                    Count = 180,
                    Moisture = 10,
                    RejectsPct = 4.5,
                    SoundPct = 65,
                    SpottedPct = 7,
                    Kor = 47,
                    Approved = Approval.Approved
                }
            };
            var inspection = new Inspection
            {
                Id = null,
                CompanyId = COMPANY_ID,
                Name = null,
                InspectionDate = new DateTime(2013, 1, 1),
                Inspector = "Dede",
                LotNo = "Lot 1234",
                TruckPlate = "AA BB CC",
                Bags = 300,
                Location = "Bouake warehouse",
                Analyses = analyses,
            };

            // Act
            var response = await sut.Save(inspection);

            // Assert
            var actual = await session.LoadAsync<Inspection>(response.Id);
            actual.Should().NotBeNull();
        }
    }
}