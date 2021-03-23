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
        public async Task DeleteInspection_ShouldDeleteInspection()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var inspection = fixture.DefaultEntity<Inspection>()
                .Without(c => c.StockReferences)
                .Create();

            await session.StoreAsync(inspection);

            // Act
            var response = await sut.DeleteInspection(inspection.Id);

            // Assert
            response.Message.Should().Be("Deleted inspection");

            using var session2 = store.OpenAsyncSession();
            var actual = await session2.LoadAsync<Inspection>(inspection.Id);
            actual.Should().BeNull();

            var query = await session2.Query<Inspection>().ToListAsync();
            query.Should().HaveCount(0);
        }

        [Fact]
        public async Task DeleteInspection_ShouldThrowExceptionIfAlreadyAllocated()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var inspection = fixture.DefaultEntity<Inspection>()
                .Create();

            await session.StoreAsync(inspection);

            // Act
            Func<Task> action = async () => await sut.DeleteInspection(inspection.Id);

            // Assert
            await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete an inspection if it has already been moved to stock");
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
        public async Task LoadList_ShouldFilterByWarehouse()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var inspections = fixture.DefaultEntity<Inspection>()
                .With(c => c.Bags, 10)
                .With(c => c.WeightKg, 5000)
                .With(c => c.StockReferences, new List<StockReference> {new("", 3, 1000, DateTime.Today, 1)})
                .Without(c => c.WarehouseId)
                .CreateMany().ToList();

            inspections[0].WarehouseId = "customers/1-A";
            await inspections.SaveList(session);

            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            var prms = new InspectionQueryParams {CompanyId = COMPANY_ID, WarehouseId = "customers/1-A"};
            var list = await sut.LoadList(prms);

            // Assert
            list.Should().HaveCount(1).And.OnlyContain(c => c.WarehouseId == "customers/1-A");
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
            var warehouse = fixture.DefaultEntity<Customer>().Create();
            var broker = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(customer);
            await session.StoreAsync(warehouse);
            await session.StoreAsync(broker);

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
                .With(c => c.Bags, 10)
                .With(c => c.WeightKg, 5000)
                .With(c => c.WarehouseId, warehouse.Id)
                .With(c => c.Price, 340)
                .With(c => c.Fiche, 00123)
                .With(c => c.Origin, "Bouake")
                .With(c => c.BuyerId, broker.Id)
                .With(c=>c.UserName,"Fred")
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
            list.Should().BeInAscendingOrder(c => c.InspectionDate);

            actual.LotNo.Should().Be(expected.LotNo);
            actual.Inspector.Should().Be(expected.Inspector);
            actual.Bags.Should().Be(expected.Bags);
            actual.WeightKg.Should().Be(expected.WeightKg);
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
            actual.AvgBagWeightKg.Should().Be(expected.AvgBagWeightKg);
            actual.Price.Should().Be(expected.Price);
            actual.WarehouseName.Should().Be(warehouse.Name);
            actual.Fiche.Should().Be(00123);
            actual.Origin.Should().Be("Bouake");
            actual.BuyerId.Should().Be(broker.Id);
            actual.BuyerName.Should().Be(broker.Name);
            actual.UserName.Should().Be("Fred");
            actual.PricePerKor.Should().Be(expected.Price / expected.AnalysisResult.Kor);
        }

        [Fact]
        public async Task LoadList_ShouldShowUnallocatedBags()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var inspection = fixture.DefaultEntity<Inspection>()
                .With(c => c.Bags, 10)
                .With(c => c.WeightKg, 5000)
                .With(c => c.StockReferences, new List<StockReference> {new("", 3, 1000, DateTime.Today, 1)})
                .Create();
            await session.StoreAsync(inspection);

            await session.SaveChangesAsync();
            WaitForIndexing(store);

            // Act
            var prms = new InspectionQueryParams {CompanyId = COMPANY_ID};
            var list = await sut.LoadList(prms);

            // Assert
            list[0].UnallocatedBags.Should().Be(inspection.Bags - inspection.StockReferences.Sum(x => x.Bags));
            list[0].UnallocatedWeightKg.Should().Be(5000 - 1000);
            list[0].UnallocatedWeightKg.Should().Be(inspection.WeightKg - inspection.StockReferences.Sum(x => x.WeightKg));
        }

        [Fact]
        public async Task LoadList_ShouldFilterByBuyers()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionService(session);
            var fixture = new Fixture();

            var buyer = fixture.DefaultEntity<Customer>().Create();
            await session.StoreAsync(buyer);

            var inspections = fixture.DefaultEntity<Inspection>()
                .Without(c => c.BuyerId)
                .CreateMany().ToList();
            inspections[0].BuyerId = buyer.Id;
            await inspections.SaveList(session);
            WaitForIndexing(store);

            // Act
            var prms = new InspectionQueryParams() {CompanyId = COMPANY_ID, BuyerId = buyer.Id};
            var actual = await sut.LoadList(prms);

            // Assert
            actual.Should().HaveCount(1).And.OnlyContain(c => c.BuyerId == buyer.Id);
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
                TruckPlate = "AA bb CC",
                Bags = 300,
                WeightKg = 29_999.0,
                Location = "Bouake warehouse",
                Analyses = analyses,
                Origin = "Firkei",
                Price = 340,
                WarehouseId = "customers/1-A",
                Fiche = 00123,
                BuyerId = "customers/3-A",
                UserName="Christian"
            };

            // Act
            var response = await sut.Save(inspection);
            await session.SaveChangesAsync();


            // Assert
            var actual = await session.LoadAsync<Inspection>(response.Id);
            actual.Should().NotBeNull();
            actual.AnalysisResult.Count.Should().Be(180);
            actual.AnalysisResult.Moisture.Should().Be(10);
            actual.Origin.Should().Be("Firkei");
            actual.WeightKg.Should().Be(29_999);
            actual.AvgBagWeightKg.Should().Be(inspection.WeightKg / inspection.Bags);
            actual.Price.Should().Be(340);
            actual.Fiche.Should().Be(00123);
            actual.TruckPlate.Should().Be("AA BB CC");
            actual.BuyerId.Should().Be("customers/3-A");
            actual.UserName.Should().Be("Christian");
        }

        // [Fact]
        // public async Task SaveShouldThrowExceptionIfFicheAlreadyExists()
        // {
        //     // Arrange
        //     using var store = GetDocumentStore();
        //     await InitializeIndexes(store);
        //     using var session = store.OpenAsyncSession();
        //     var sut = GetInspectionService(session);
        //     var fixture = new Fixture();
        //
        //     var inspection = fixture.DefaultEntity<Inspection>()
        //         .With(c => c.Fiche, 123)
        //         .Create();
        //     await session.StoreAsync(inspection);
        //     await session.SaveChangesAsync();
        //     
        //     var test = fixture.DefaultEntity<Inspection>()
        //         .With(c => c.Fiche, 123)
        //         .Create();
        //
        //     // Act
        //     Func<Task> act = async () => await sut.Save(test);
        //
        //     // Assert
        //     act.Should().Throw<InvalidOperationException>().WithMessage("There is already an inspection in the system with fiche de transfer 123");
        // }
    }
}