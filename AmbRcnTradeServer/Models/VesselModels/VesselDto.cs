using System.Collections.Generic;
using AmberwoodCore.Interfaces;
using AmbRcnTradeServer.Models.ContainerModels;

namespace AmbRcnTradeServer.Models.VesselModels
{
    public class VesselDto : Vessel, IEntityDto
    {
        [AutoMapper.IgnoreMap]
        public List<Container> Containers { get; set; } = new();

        public void Validate() { }
    }
}