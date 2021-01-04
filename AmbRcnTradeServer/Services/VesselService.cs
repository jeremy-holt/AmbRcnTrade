using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.VesselModels;
using AmbRcnTradeServer.RavenIndexes;
using AutoMapper;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IVesselService
    {
        Task<ServerResponse<VesselDto>> Save(VesselDto vesselDto);
        Task<VesselDto> Load(string id);
        Task<List<VesselListItem>> LoadList(string companyId);
        Task<List<NotLoadedContainer>> GetNotLoadedContainers(string companyId);
        Task<ServerResponse> AddContainerToVessel(string vesselId, IEnumerable<string> containerIds);
        Task<ServerResponse<VesselDto>> RemoveContainersFromVessel(string vesselId, IEnumerable<string> containerIds);
    }

    public class VesselService : IVesselService
    {
        private readonly IMapper _mapper;
        private readonly IAsyncDocumentSession _session;

        public VesselService(IAsyncDocumentSession session, IMapper mapper)
        {
            _session = session;
            _mapper = mapper;
        }

        public async Task<ServerResponse<VesselDto>> Save(VesselDto vesselDto)
        {
            var vessel = vesselDto.Id.IsNullOrEmpty() ? new Vessel() : await _session.LoadAsync<Vessel>(vesselDto.Id);

            _mapper.Map(vesselDto, vessel);
            vessel.ContainersOnBoard = vesselDto.ContainerIds.Count;

            await _session.StoreAsync(vessel);
            vesselDto.Id = vessel.Id;

            return new ServerResponse<VesselDto>(vesselDto, "Saved");
        }

        public async Task<VesselDto> Load(string id)
        {
            var vessel = await _session
                .Include<Vessel>(c => c.ContainerIds)
                .LoadAsync<Vessel>(id);

            var containers = await _session.LoadListFromMultipleIdsAsync<Container>(vessel.ContainerIds);


            var vesselDto = new VesselDto();
            _mapper.Map(vessel, vesselDto);

            vesselDto.Containers = containers;

            return vesselDto;
        }

        public async Task<List<VesselListItem>> LoadList(string companyId)
        {
            var query = await _session.Query<Vessel>()
                .Where(c => c.CompanyId == companyId)
                .Select(c => new VesselListItem
                {
                    Id = c.Id,
                    BlDate = c.BlDate,
                    BlNumber = c.BlNumber,
                    ForwardingAgent = c.ForwardingAgent,
                    ShippingCompany = c.ShippingCompany,
                    ContainersOnBoard = c.ContainersOnBoard,
                    Eta = c.EtaHistory.FirstOrDefault(hist => hist.DateUpdated == c.EtaHistory.Max(x => x.DateUpdated)).Eta,
                    VesselName = c.EtaHistory.FirstOrDefault(hist => hist.DateUpdated == c.EtaHistory.Max(x => x.DateUpdated)).VesselName
                })
                .OrderBy(c => c.BlDate)
                .ToListAsync();

            return query;
        }

        public async Task<List<NotLoadedContainer>> GetNotLoadedContainers(string companyId)
        {
            var query = await _session.Query<NotLoadedContainer, Containers_ByVessel>()
                .Where(c => c.CompanyId == companyId &&
                            string.IsNullOrEmpty(c.VesselId) &&
                            !c.Status.In(ContainerStatus.Cancelled, ContainerStatus.OnBoardVessel))
                .ProjectInto<NotLoadedContainer>()
                .ToListAsync();

            return query;
        }

        public async Task<ServerResponse> AddContainerToVessel(string vesselId, IEnumerable<string> containerIds)
        {
            var vessel = await _session.LoadAsync<Vessel>(vesselId);
            foreach (var containerId in containerIds)
            {
                if(!vessel.ContainerIds.Contains(containerId))
                    vessel.ContainerIds.Add(containerId);
            }

            return new ServerResponse("Added containers");
        }

        public async Task<ServerResponse<VesselDto>> RemoveContainersFromVessel(string vesselId, IEnumerable<string> containerIds)
        {
            var vessel = await _session.LoadAsync<Vessel>(vesselId);
            vessel.ContainerIds.RemoveAll(c => c.In(containerIds));

            var dto = await Load(vesselId);
            return new ServerResponse<VesselDto>(dto, "Removed containers");
        }
    }
}