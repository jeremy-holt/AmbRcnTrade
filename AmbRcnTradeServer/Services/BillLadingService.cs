﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models.ContainerModels;
using AmbRcnTradeServer.Models.DraftBillLadingModels;
using AmbRcnTradeServer.Models.VesselModels;
using AmbRcnTradeServer.RavenIndexes;
using AutoMapper;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IBillLadingService
    {
        Task<ServerResponse> AddContainersToBillLading(string billLadingId, string vesselId, List<string> containerIds);
        Task<ServerResponse<BillLadingDto>> Save(BillLadingDto billLadingDto);
        Task<BillLadingDto> Load(string id);
        Task<List<BillLadingListItem>> LoadList(string companyId);
        Task<List<Container>> GetNotLoadedContainers(string companyId);
        Task<ServerResponse<BillLadingDto>> RemoveContainersFromBillLading(string billLadingId, IEnumerable<string> containerIds);
        Task<ServerResponse> MoveBillLadingToVessel(string billLadingId, string fromVesselId, string toVesselId);
        CargoDescription GetPreCargoDescription(double? numberBags, double numberContainers, double? grossWeightKg, string productDescription, Teu teu, string declarationNumber);
        Task<ServerResponse> DeleteBillLading(string vesselId, string billLadingId);
    }

    public class BillLadingService : IBillLadingService
    {
        private readonly IMapper _mapper;
        private readonly IAsyncDocumentSession _session;

        public BillLadingService(IAsyncDocumentSession session, IMapper mapper)
        {
            _session = session;
            _mapper = mapper;
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

            if (billLading.Id.IsNullOrEmpty())
            {
                InitializeDocuments(billLadingDto);
            }

            var vessel = await _session.LoadAsync<Vessel>(billLading.VesselId);

            _mapper.Map(billLadingDto, billLading);
            billLading.ContainersOnBoard = billLadingDto.ContainerIds.Count;
            billLading.NumberBags = billLadingDto.Containers.Sum(x => x.Bags);
            billLading.NettWeightKg = billLadingDto.Containers.Sum(x => x.NettWeightKg);
            billLading.GrossWeightKg = billLadingDto.Containers.Sum(x => x.WeighbridgeWeightKg);
            billLading.NumberPackagesText = billLading.NumberBags == null ? "" : $"{billLading.NumberBags} PACKAGES";
            billLading.NettWeightKgText = billLading.NettWeightKg == null ? "" : $"{billLading.NettWeightKg:N0} KGS";
            billLading.GrossWeightKgText = billLading.GrossWeightKg == null ? "" : $"{billLading.GrossWeightKg:N0} KGS";
            billLading.VgmWeightKgText = billLading.GrossWeightKgText;
            billLading.PreCargoDescription = GetPreCargoDescription(billLading.NumberBags, billLading.ContainersOnBoard, billLading.GrossWeightKg, billLading.ProductDescription,
                billLading.Teu, billLading.DeclarationNumber);

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
                .Include(c => c.ConsigneeId)
                .Include(c => c.ShipperId)
                .Include(c => c.DestinationAgentId)
                .Include(c => c.NotifyParty1Id)
                .Include(c => c.NotifyParty2Id)
                .Include(c => c.PortOfDestinationId)
                .LoadAsync<BillLading>(id);

            var containers = await _session.LoadListFromMultipleIdsAsync<Container>(billLading.ContainerIds);

            var billLadingDto = new BillLadingDto();
            _mapper.Map(billLading, billLadingDto);

            billLadingDto.Containers = containers;

            if (billLadingDto.Documents.Count == 0)
                InitializeDocuments(billLadingDto);

            return billLadingDto;
        }

        public async Task<List<Container>> GetNotLoadedContainers(string companyId)
        {
            var allowed = new[] {ContainerStatus.OnWayToPort, ContainerStatus.Gated, ContainerStatus.StuffingComplete};

            var query = await _session.Query<Container, Containers_Available_ForBillLading>()
                .Where(c => c.CompanyId == companyId && c.Status.In(allowed))
                .ToListAsync();

            return query;
        }

        public async Task<ServerResponse> AddContainersToBillLading(string billLadingId, string vesselId, List<string> containerIds)
        {
            var billLading = await _session.LoadAsync<BillLading>(billLadingId);

            foreach (var containerId in containerIds)
            {
                if (!billLading.ContainerIds.Contains(containerId))
                    billLading.ContainerIds.Add(containerId);
            }


            var containers = await _session.LoadListFromMultipleIdsAsync<Container>(containerIds);

            foreach (var container in containers)
            {
                container.Status = ContainerStatus.OnBoardVessel;
                container.VesselId = vesselId;
            }

            await _session.SaveChangesAsync();

            return new ServerResponse("Added container(s) to Bill of Lading");
        }

        public async Task<ServerResponse<BillLadingDto>> RemoveContainersFromBillLading(string billLadingId, IEnumerable<string> containerIds)
        {
            var billLading = await _session
                .Include<BillLading>(c => c.ContainerIds)
                .LoadAsync<BillLading>(billLadingId);

            billLading.ContainerIds.RemoveAll(c => c.In(containerIds));

            var removedContainers = await _session.LoadListFromMultipleIdsAsync<Container>(containerIds);
            foreach (var container in removedContainers)
            {
                container.Status = ContainerStatus.Gated;
                container.VesselId = null;
            }

            var dto = await Load(billLadingId);
            return new ServerResponse<BillLadingDto>(dto, "Removed containers from Bill of Lading");
        }

        public async Task<ServerResponse> MoveBillLadingToVessel(string billLadingId, string fromVesselId, string toVesselId)
        {
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
            await _session.SaveChangesAsync();
            return new ServerResponse("Moved Bill of Lading");
        }

        public CargoDescription GetPreCargoDescription(double? numberBags, double numberContainers, double? grossWeightKg, string productDescription, Teu teu,
            string declarationNumber)
        {
            var teuText = teu switch
            {
                Teu.Teu20 => "20HC",
                Teu.Teu40 => "40HC",
                _ => ""
            };

            var bodyText =
                $"{numberContainers}X{teuText} CONTAINER(S) SAID TO CONTAIN:\n" +
                $"{numberBags} JUTE BAGS OF DRIED RAW CASHEW NUTS IN SHELL\n" +
                "OF IVORY COAST ORIGIN - 2021 NEW CROP\n" +
                "HS CODE: 08013100";

            var weightsText = $"IN {numberBags} JUTE BAGS\n" +
                              $"GROSS WEIGHT: {grossWeightKg:N0} KGS\n" +
                              $"LESS WEIGHT OF EMPTY BAGS: {numberBags} KGS\n" +
                              $"NET WEIGHT: {grossWeightKg - numberBags:N0} KGS\n" +
                              "FREIGHT PREPAID\n" +
                              $"DECLARATION NO: {declarationNumber}\n" +
                              "21 FREE DAYS AT PORT OF DESTINATION";
            return new CargoDescription {Header = bodyText, Footer = weightsText};
        }

        public async Task<ServerResponse> DeleteBillLading(string vesselId, string billLadingId)
        {
            var vessel = await _session
                .Include<Vessel>(c => c.BillLadingIds)
                .LoadAsync<Vessel>(vesselId);

            var billLading = await _session
                .Include<BillLading>(c => c.ContainerIds)
                .LoadAsync<BillLading>(billLadingId);

            var containers = await _session.LoadListFromMultipleIdsAsync<Container>(billLading.ContainerIds);

            vessel.BillLadingIds.Remove(billLadingId);

            foreach (var container in containers)
                container.Status = ContainerStatus.Gated;


            _session.Delete(billLading);

            return new ServerResponse("Deleted Bill of Lading");
        }

        private static void InitializeDocuments(BillLading billLadingDto)
        {
            billLadingDto.Documents.AddRange(new List<Document>
            {
                new("ACE certificate of weight and quality"),
                new("CCA redevance"),
                new("Customs duties"),
                new("Phytosanitary certificate"),
                new("Fumigation certificate"),
                new("Certificate of origin")
            });
        }
    }
}