using System;

namespace AmbRcnTradeServer.Models.VesselModels
{
    public class Document
    {
        public Document(string name)
        {
            Name = name;
        }

        public Document() { }
        public string Name { get; set; }
        public DateTime? Submitted { get; set; }
        public DateTime? Received { get; set; }
        public string Notes { get; set; }
    }
}