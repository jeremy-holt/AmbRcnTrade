using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
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
        Task<ServerResponse<VesselDto>> RemoveBillsLadingFromVessel(string vesselId, IEnumerable<string> billLadingIds);
        Task<ServerResponse> AddBillLadingToVessel(string vesselId, IEnumerable<string> billLadingIds);
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
            // var bills = await _session.LoadListFromMultipleIdsAsync<BillLading>(vessel.BillLadingIds);
            // foreach (var bl in bills)
            // {
            //     bl.ContainersOnBoard = bl.ContainerIds.Count();
            // }
            //
            _mapper.Map(vesselDto, vessel);
            vessel.ContainersOnBoard = vesselDto.BillLadings.Sum(x => x.ContainerIds.Count);

            await _session.StoreAsync(vessel);
            vesselDto.Id = vessel.Id;

            return new ServerResponse<VesselDto>(vesselDto, "Saved");
        }

        public async Task<VesselDto> Load(string id)
        {
            var vessel = await _session
                .Include<Vessel>(c => c.BillLadingIds)
                .LoadAsync<Vessel>(id);

            var billLadings = await _session.LoadListFromMultipleIdsAsync<BillLading>(vessel.BillLadingIds);
            foreach (var bl in billLadings)
                bl.ContainersOnBoard = bl.ContainerIds.Count();

            var vesselDto = new VesselDto();
            _mapper.Map(vessel, vesselDto);

            vesselDto.BillLadings = billLadings;
            vessel.ContainersOnBoard = billLadings.Sum(c => c.ContainersOnBoard);

            return vesselDto;
        }

        public async Task<List<VesselListItem>> LoadList(string companyId)
        {
            return await _session.Query<VesselListItem, Vessels_ByCustomers>()
                .Where(c => c.CompanyId == companyId)
                .ProjectInto<VesselListItem>()
                .OrderBy(c => c.Eta).ToListAsync();
        }

        public async Task<ServerResponse<VesselDto>> RemoveBillsLadingFromVessel(string vesselId, IEnumerable<string> billLadingIds)
        {
            var vessel = await _session.LoadAsync<Vessel>(vesselId);
            vessel.BillLadingIds.RemoveAll(c => c.In(billLadingIds));

            var dto = await Load(vesselId);
            return new ServerResponse<VesselDto>(dto, "Removed Bills of Lading from vessel");
        }

        public async Task<ServerResponse> AddBillLadingToVessel(string vesselId, IEnumerable<string> billLadingIds)
        {
            var vessel = await _session.LoadAsync<Vessel>(vesselId);
            foreach (var blId in billLadingIds)
            {
                if (!vessel.BillLadingIds.Contains(blId))
                    vessel.BillLadingIds.Add(blId);
            }

            return new ServerResponse("Added Bill of Lading to vessel");
        }
    }
}