using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.PurchaseModels;
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
    public class BagDeliveryServiceTests : TestBaseContainer
    {
        public BagDeliveryServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public async Task SaveBagDelivery_ShouldSaveBagsToSupplier()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBagDeliveryService(session);
            var fixture = new Fixture();

            var bagDelivery = fixture.DefaultEntity<BagDelivery>()
                .With(c=>c.DeliveryDate,new DateTime(2013,1,1))
                .With(c=>c.SupplierId,"customers/1-A")
                .With(c=>c.NumberBags,500)
                .With(c=>c.BagType,BagType.Export)
                .With(c=>c.Notes,"notes")
                .Create();

            // Act
            var response = await sut.Save(bagDelivery);

            // Assert
            var actual = await session.LoadAsync<BagDelivery>(response.Id);
            actual.Should().BeEquivalentTo(bagDelivery);
            actual.NumberBags.Should().Be(500);
            actual.Notes.Should().Be("notes");
        }

        [Fact]
        public async Task LoadBagDelivery_ShouldLoadBagDelivery()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBagDeliveryService(session);
            var fixture = new Fixture();

           var bagDelivery =  await fixture.DefaultEntity<BagDelivery>().Create().CreateAndStoreAsync(session);
           
           // Act
           BagDelivery actual = await sut.Load(bagDelivery.Id);
           
           // Assert
           actual.NumberBags.Should().Be(bagDelivery.NumberBags);
        }

        [Fact]
        public async Task LoadList_ShouldReturnListOfBagDeliveries()
        {
            // Arrange
            using var store = GetDocumentStore();
            using var session = store.OpenAsyncSession();
            var sut = GetBagDeliveryService(session);
            var fixture = new Fixture();

            var suppliers = fixture.DefaultEntity<Customer>().CreateMany().ToList();
            await suppliers.SaveList(session);

            var deliveries = fixture.DefaultEntity<BagDelivery>().CreateMany().ToList();
            deliveries[0].SupplierId = suppliers[0].Id;
            deliveries[1].SupplierId = suppliers[1].Id;
            deliveries[2].SupplierId = suppliers[2].Id;
            await deliveries.SaveList(session);
            
            WaitForIndexing(store);

            var purchaseListItems = fixture.Build<PurchaseListItem>().CreateMany().ToList();
            purchaseListItems[0].SupplierId = suppliers[0].Id;
            purchaseListItems[1].SupplierId = suppliers[1].Id;
            purchaseListItems[2].SupplierId = suppliers[2].Id;
            
            // Act
            List<BagDeliveryListItem> list = await sut.LoadList(COMPANY_ID, suppliers[0].Id);
            
            // Assert
            list.Should().OnlyContain(c => c.SupplierId == suppliers[0].Id);

        }
    }
}