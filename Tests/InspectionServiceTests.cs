using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Models;
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

            var inspections = fixture.DefaultEntity<Inspection>()
                .With(c => c.Approved, Approval.Approved)
                .With(c => c.SupplierId, customer.Id)
                .CreateMany()
                .ToList();
            inspections[2].Approved = Approval.Rejected;

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

            var actual = list.Single(c => c.InspectionId == inspections[0].Id);
            var expected = inspections[0];

            // Assert
            list.Should().OnlyContain(c => c.Approved == prms.Approved);
            list.Should().BeInDescendingOrder(c => c.InspectionDate);

            actual.LotNo.Should().Be(expected.LotNo);
            actual.Inspector.Should().Be(expected.Inspector);
            actual.Bags.Should().Be(expected.Bags);
            actual.ApproxWeight.Should().Be(expected.ApproxWeight);
            actual.Approved.Should().Be(expected.Approved);
            actual.Location.Should().Be(expected.Location);
            actual.TruckPlate.Should().Be(expected.TruckPlate);
            actual.SupplierName.Should().Be(customer.Name);
            actual.SupplierId.Should().Be(customer.Id);
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
                    Rejects = 4.5,
                    Sound = 65,
                    Spotted = 7,
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
                ApproxWeight = 24000,
                Location = "Bouake warehouse",
                Analyses = analyses,
                Approved = Approval.Approved
            };

            // Act
            var response = await sut.Save(inspection);

            // Assert
            var actual = await session.LoadAsync<Inspection>(response.Id);
            actual.Should().NotBeNull();
        }

        [Fact]
        public async Task Save_ShouldCalculateAnalysisResultFromTests()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var analyses = new List<Analysis>
            {
                new()
                {
                    Bags = 100,
                    Count = 200,
                    Moisture = 10,
                    Rejects = 10,
                    Sound = 60,
                    Spotted = 10,
                    Kor = 50,
                    Approved = Approval.Approved
                },
                new()
                {
                    Bags = 100,
                    Count = 220,
                    Moisture = 14,
                    Rejects = 14,
                    Sound = 70,
                    Spotted = 14,
                    Kor = 54,
                    Approved = Approval.Approved
                },
                new()
                {
                    Bags = 999,
                    Count = 999,
                    Moisture = 999,
                    Rejects = 999,
                    Sound = 999,
                    Spotted = 999,
                    Kor = 999,
                    Approved = Approval.Rejected
                },
            };

            var inspection = fixture.DefaultEntity<Inspection>()
                .With(c => c.Analyses, analyses)
                .Without(c => c.AnalysisResult)
                .With(c => c.Bags, 300)
                .Create();

            // Act
            var response = await sut.Save(inspection);
            
            var actual = await sut.Load(inspection.Id);

            // Assert
            actual.AnalysisResult.Count.Should().Be(210);
            actual.AnalysisResult.Moisture.Should().Be(12);
            actual.AnalysisResult.Rejects.Should().Be(12);
            actual.AnalysisResult.Sound.Should().Be(65);
            actual.AnalysisResult.Spotted.Should().Be(12);
            actual.AnalysisResult.Kor.Should().Be(52);
        }
    }
}