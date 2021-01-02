using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
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
    public class InspectionServiceTests : TestBaseContainer
    {
        public InspectionServiceTests(ITestOutputHelper output) : base(output) { }

        private static async Task InitializeIndexes(IDocumentStore store)
        {
            await new Inspections_ByAnalysisResult().ExecuteAsync(store);
        }

        [Fact]
        public async Task GetAnalysisResult_ShouldReturnAverageAnalysisResultForInspectionIds()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            await InitializeIndexes(store);
            var fixture = new Fixture();

            var analysis1 = new Analysis
            {
                Count = 190,
                Moisture = 7,
                RejectsGm = 130,
                SoundGm = 240,
                SpottedGm = 16
            };
            var analysis2 = new Analysis
            {
                Count = 190,
                Moisture = 7,
                RejectsGm = 130,
                SoundGm = 240,
                SpottedGm = 16
            };

            var inspections = fixture.DefaultEntity<Inspection>()
                .With(c => c.Analyses, new List<Analysis> {analysis1, analysis2})
                .CreateMany()
                .ToList();

            await inspections.SaveList(session);
            WaitForIndexing(store);

            // Act
            var actual = await sut.GetAnalysisResult(inspections.Select(c => c.Id));

            // Assert
            actual[0].Approved.Should().Be(inspections[0].AnalysisResult.Approved);
            actual[0].Count.Should().Be(inspections[0].Analyses.Average(c => c.Count));
            actual[0].InspectionId.Should().Be(inspections[0].Id);
        }

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

            var analysisResult1 = fixture.Build<AnalysisResult>()
                .With(c => c.Approved, Approval.Approved)
                .Create();
            var analysisResult2 = fixture.Build<AnalysisResult>()
                .With(c => c.Approved, Approval.Approved)
                .Create();
            var analysisResult3 = fixture.Build<AnalysisResult>()
                .With(c => c.Approved, Approval.Rejected)
                .Create();

            var inspections = fixture.DefaultEntity<Inspection>()
                .Without(c => c.AnalysisResult)
                .With(c => c.SupplierId, customer.Id)
                .CreateMany()
                .ToList();
            inspections[0].AnalysisResult = analysisResult1;
            inspections[1].AnalysisResult = analysisResult2;
            inspections[2].AnalysisResult = analysisResult3;

            await inspections.SaveList(session);
            await session.SaveChangesAsync();
            WaitForIndexing(store);

            var prms = new InspectionQueryParams
            {
                CompanyId = COMPANY_ID,
                Approved = Approval.Approved
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
            actual.StockReferences.Should().HaveCount(3);
            actual.StockAllocations.Should().Be(3);
        }

        [Fact]
        public async Task LoadList_ShouldShowUnallocatedBags()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var inspection = fixture.DefaultEntity<Inspection>().Create();
            await session.StoreAsync(inspection);

            await session.SaveChangesAsync();
            WaitForIndexing(store);
            
            // Act
            var prms = new InspectionQueryParams() {CompanyId = COMPANY_ID};
            var list = await sut.LoadList(prms);
            
            // Assert
            list[0].UnallocatedBags.Should().Be(inspection.Bags - inspection.StockReferences.Sum(x => x.Bags));

        }

        [Fact]
        public async Task Save_ShouldSaveAnInspection()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            await InitializeIndexes(store);
            var sut = GetInspectionService(session);

            var analyses = new List<Analysis>
            {
                new()
                {
                    Count = 180,
                    Moisture = 10,
                    Kor = 47
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
                Analyses = analyses
            };

            // Act
            var response = await sut.Save(inspection);
            await session.SaveChangesAsync();
            

            // Assert
            var actual = await session.LoadAsync<Inspection>(response.Id);
            actual.Should().NotBeNull();
            actual.AnalysisResult.Count.Should().Be(180);
            actual.AnalysisResult.Moisture.Should().Be(10);
        }
    }
}