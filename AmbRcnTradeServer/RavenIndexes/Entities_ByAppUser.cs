﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.DictionaryModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.RavenIndexes
{
    public class Entities_ByAppUser : AbstractMultiMapIndexCreationTask<Entities_ByAppUser.Result>
    {
        public Entities_ByAppUser()
        {
            AddMap<Contract>(contracts => from c in contracts
                let customer = LoadDocument<Customer>(c.SellerId)
                select new
                {
                    c.CompanyId,
                    c.Id,
                    CustomerId = customer.Id,
                    Users = customer.Users.Select(x => x.AppUserId),
                    CustomerName = customer.CompanyName,
                    Group = customer.Filter
                }
            );
            AddMap<Contract>(contracts => from c in contracts
                let customer = LoadDocument<Customer>(c.BuyerId)
                select new
                {
                    c.CompanyId,
                    c.Id,
                    CustomerId = customer.Id,
                    Users = customer.Users.Select(x => x.AppUserId),
                    CustomerName = customer.CompanyName,
                    Group = customer.Filter
                }
            );
            AddMap<Contract>(contracts => from c in contracts
                let customer = LoadDocument<Customer>(c.BrokerId)
                select new
                {
                    c.CompanyId,
                    c.Id,
                    CustomerId = customer.Id,
                    Users = customer.Users.Select(x => x.AppUserId),
                    CustomerName = customer.CompanyName,
                    Group = customer.Filter
                }
            );

            Index(x => x.CompanyId, FieldIndexing.Default);
            Index(x => x.Users, FieldIndexing.Default);

            StoreAllFields(FieldStorage.Yes);
        }

        public class Result
        {
            public string CompanyId { get; set; }
            public string Id { get; set; }
            public IEnumerable<string> Users { get; set; }

            public string CustomerName { get; set; }
            public string CustomerId { get; set; }
            public CustomerGroup Filter { get; set; }
        }
    }
}