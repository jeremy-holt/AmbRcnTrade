using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Services;
using AutoFixture;
using FluentAssertions;
using GemBox.Spreadsheet;
using Raven.Client.Documents.Session;
using Tests.Base;
using Xunit;
using Xunit.Abstractions;

namespace Tests
{
    public class InspectExportExcelTests: TestBaseContainer
    {
        public InspectExportExcelTests(ITestOutputHelper output) : base(output) { }
        private const string InspectionExportTemplate = "InspectionExportTemplate.xlsx";

        [Fact]
        public void LoadTemplate_ShouldLoadInspectionExportTemplate()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            IInspectionExportService sut =(InspectionExportService) GetInspectionExportService();
            
            // Act
            var workbook = sut.LoadTemplate(InspectionExportTemplate);
            
            // Assert
            workbook.Should().NotBeNull();
        }

        [Fact]
        public void LoadData_ShouldLoadData()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionExportService();
            var fixture = new Fixture();

            var inspections = fixture.CreateMany<InspectionListItem>().ToList();
            inspections[0].Id = "inspections/1-A";
            inspections[1].Id = "inspections/2-A";
            inspections[2].Id = "inspections/3-A";
            
            // Act
            ExcelFile workbook = sut.GetWorkbook(InspectionExportTemplate, inspections);
            
            // Assert
            workbook.Should().NotBeNull();
        }

        [Fact]
        public async Task SaveWorkbook_ShouldSaveTheWorkbook()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetInspectionExportService();
            var fixture = new Fixture();
            
            var inspections = fixture
                .Build<InspectionListItem>()
                .With(c=>c.Id,"inspections/1-A")
                .CreateMany().ToList();

            // Act
            ExcelFile workbook = sut.GetWorkbook(InspectionExportTemplate, inspections);
            var actual = await sut.SaveWorkbook(workbook);
            
            // Assert
            actual.ContentType.Should().Be(SaveOptions.XlsxDefault.ContentType);
        }

        private static IInspectionExportService GetInspectionExportService()
        {
            return new InspectionExportService();
        }
    }
}