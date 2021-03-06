﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.AppUserModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.DictionaryModels
{
    public class CustomerUserListItem
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<User> Users { get; set; } = new();
        public string Name { get; set; }
    }
}