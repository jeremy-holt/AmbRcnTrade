﻿using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Interfaces;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Models.AppUserModels
{
    public class AppUserPassword : IEntity
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string AppUserId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}