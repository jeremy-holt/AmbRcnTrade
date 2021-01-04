using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Initializations;
using AmbRcnTradeServer.Models.VesselModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Startup_Config
{
    public class ClientMappingProfile : ClientMappingProfileBase
    {
        public ClientMappingProfile()
        {
            CreateMap<Vessel, VesselDto>().ReverseMap();
        }
    }
}