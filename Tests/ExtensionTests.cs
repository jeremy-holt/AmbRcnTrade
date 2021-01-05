using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Interfaces;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.Models.StockModels;
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
    public class ExtensionTests : TestBaseContainer
    {
        public ExtensionTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void AverageAnalysisResults_ShouldAverageListOfAnalysisResults()
        {
            // Arrange
            var fixture = new Fixture();

            var analysisResults = fixture.Build<AnalysisResult>().CreateMany().ToList();
            analysisResults[0].Approved = Approval.Approved;
            analysisResults[1].Approved = Approval.Approved;
            analysisResults[2].Approved = Approval.Rejected;

            var stock1 = fixture.DefaultEntity<Stock>().With(c => c.AnalysisResult, analysisResults[0]).Create();
            var stock2 = fixture.DefaultEntity<Stock>().With(c => c.AnalysisResult, analysisResults[1]).Create();
            var stock3 = fixture.DefaultEntity<Stock>().With(c => c.AnalysisResult, analysisResults[2]).Create();

            IEnumerable<IAnalysisResult> stocks = new[] {stock1, stock2, stock3};

            // Act
            var actual = stocks.AverageAnalysisResults();

            // Assert
            actual.Approved.Should().Be(Approval.Approved);
            actual.Count.Should().Be((analysisResults[0].Count + analysisResults[1].Count) / 2);
        }
    }
}