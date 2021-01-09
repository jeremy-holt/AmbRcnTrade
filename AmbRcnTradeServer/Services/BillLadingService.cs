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
using AutoMapper.Configuration.Annotations;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IBillLadingService
    {
        Task<ServerResponse> AddContainersToBillLading(string billLadingId, IEnumerable<string> containerIds);
        Task<ServerResponse<BillLadingDto>> Save(BillLadingDto billLadingDto);
        Task<BillLadingDto> Load(string id);
        Task<List<BillLadingListItem>> LoadList(string companyId);
        Task<List<Container>> GetNotLoadedContainers(string companyId);
        Task<ServerResponse<BillLadingDto>> RemoveContainersFromBillLading(string billLadingId, IEnumerable<string> containerIds);
        Task<ServerResponse> MoveBillLadingToVessel(string billLadingId, string fromVesselId, string toVesselId);
    }

    public class BillLadingService : IBillLadingService
    {
        private readonly IMapper _mapper;
        private readonly IAsyncDocumentSession _session;
        private readonly IVesselService _vesselService;

        public BillLadingService(IAsyncDocumentSession session, IMapper mapper, IVesselService vesselService)
        {
            _session = session;
            _mapper = mapper;
            _vesselService = vesselService;
        }

        public async Task<List<BillLadingListItem>> LoadList(string companyId)
        {
            var query = await _session.Query<BillLadingListItem, BillsOfLading_ByCustomers>()
                .Where(c => c.CompanyId == companyId)
                .ProjectInto<BillLadingListItem>()
                .OrderBy(c => c.BlDate)
                .ToListAsync();
            return query;
        }

        public async Task<ServerResponse<BillLadingDto>> Save(BillLadingDto billLadingDto)
        {
            var billLading = billLadingDto.Id.IsNullOrEmpty()
                ? new BillLading {VesselId = billLadingDto.VesselId, CompanyId = billLadingDto.CompanyId}
                : await _session.Include<BillLading>(c => c.VesselId).LoadAsync<BillLading>(billLadingDto.Id);

            var vessel = await _session.LoadAsync<Vessel>(billLading.VesselId);

            _mapper.Map(billLadingDto, billLading);
            billLading.ContainersOnBoard = billLadingDto.ContainerIds.Count;

            await _session.StoreAsync(billLading);
            billLadingDto.Id = billLading.Id;

            if (!vessel.BillLadingIds.Contains(billLading.Id) && billLading.Id.IsNotNullOrEmpty())
                vessel.BillLadingIds.Add(billLading.Id);

            return new ServerResponse<BillLadingDto>(billLadingDto, "Saved");
        }

        public async Task<BillLadingDto> Load(string id)
        {
            var billLading = await _session
                .Include<BillLading>(c => c.ContainerIds)
                .LoadAsync<BillLading>(id);

            var containers = await _session.LoadListFromMultipleIdsAsync<Container>(billLading.ContainerIds);

            var billLadingDto = new BillLadingDto();
            _mapper.Map(billLading, billLadingDto);

            billLadingDto.Containers = containers;

            return billLadingDto;
        }

        public async Task<List<Container>> GetNotLoadedContainers(string companyId)
        {
            var allowed = new[] {ContainerStatus.OnWayToPort, ContainerStatus.Gated, ContainerStatus.StuffingComplete};

            var query = await _session.Query<Container, Containers_Available_ForBillLading>()
                .Where(c => c.CompanyId == companyId && c.Status.In(allowed))
                // .ProjectInto<Container>()
                .ToListAsync();

            return query;
        }

        public async Task<ServerResponse> AddContainersToBillLading(string billLadingId, IEnumerable<string> containerIds)
        {
            var billLading = await _session.LoadAsync<BillLading>(billLadingId);
            foreach (var containerId in containerIds)
            {
                if (!billLading.ContainerIds.Contains(containerId))
                    billLading.ContainerIds.Add(containerId);
            }

            return new ServerResponse("Added container(s) to Bill of Lading");
        }

        public async Task<ServerResponse<BillLadingDto>> RemoveContainersFromBillLading(string billLadingId, IEnumerable<string> containerIds)
        {
            var billLading = await _session.LoadAsync<BillLading>(billLadingId);
            billLading.ContainerIds.RemoveAll(c => c.In(containerIds));

            var dto = await Load(billLadingId);
            return new ServerResponse<BillLadingDto>(dto, "Removed containers from Bill of Lading");
        }

        public async Task<ServerResponse> MoveBillLadingToVessel(string billLadingId, string fromVesselId, string toVesselId)
        {
            // var fromVessel = await _vesselService.Load(fromVesselId);
            // var toVessel = await _vesselService.Load(toVesselId);

            var fromVessel = await _session.Include<Vessel>(c => c.BillLadingIds)
                .LoadAsync<Vessel>(fromVesselId);


            var toVessel = await _session.Include<Vessel>(c => c.BillLadingIds)
                .LoadAsync<Vessel>(toVesselId);

            var billLadingIds = fromVessel.BillLadingIds.Concat(toVessel.BillLadingIds);
            var billLadings = await _session.LoadListFromMultipleIdsAsync<BillLading>(billLadingIds);

            foreach (var bl in billLadings)
            {
                bl.ContainersOnBoard = bl.ContainerIds.Count();
                await _session.StoreAsync(bl);
            }

            fromVessel.BillLadingIds.Remove(billLadingId);

            if (!toVessel.BillLadingIds.Contains(billLadingId))
                toVessel.BillLadingIds.Add(billLadingId);

            var fromVesselContainersOnBoard = billLadings.Where(c => c.Id.In(fromVessel.BillLadingIds)).Sum(x => x.ContainersOnBoard);
            var toVesselContainersOnBoard = billLadings.Where(c => c.Id.In(toVessel.BillLadingIds)).Sum(x => x.ContainersOnBoard);

            fromVessel.ContainersOnBoard = fromVesselContainersOnBoard;
            toVessel.ContainersOnBoard = toVesselContainersOnBoard;

            await _session.StoreAsync(fromVessel);
            await _session.StoreAsync(toVessel);
            // await _vesselService.Save(fromVessel);
            // await _vesselService.Save(toVessel);
            await _session.SaveChangesAsync();
            return new ServerResponse("Moved Bill of Lading");
        }
    }
}