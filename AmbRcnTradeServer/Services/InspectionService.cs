using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
using AmbRcnTradeServer.RavenIndexes;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IInspectionService
    {
        Task<ServerResponse<Inspection>> Save(Inspection inspection);
        Task<Inspection> Load(string id);
        Task<List<InspectionListItem>> LoadList(InspectionQueryParams prms);
        Task<List<AnalysisResult>> GetAnalysisResult(IEnumerable<string> inspectionIds);
        Task<AnalysisResult> GetAnalysisResult(string inspectionId);
        Task<ServerResponse> DeleteInspection(string id);
    }

    public class InspectionService : IInspectionService
    {
        private readonly IAsyncDocumentSession _session;

        public InspectionService(IAsyncDocumentSession session)
        {
            _session = session;
            _session.Advanced.WaitForIndexesAfterSaveChanges(new TimeSpan(0, 0, 30));
        }

        public async Task<ServerResponse<Inspection>> Save(Inspection inspection)
        {
            inspection.AvgBagWeightKg = inspection.Bags > 0 ? inspection.WeightKg / inspection.Bags : 0;

            await _session.StoreAsync(inspection);
            await _session.SaveChangesAsync();
            inspection.AnalysisResult = await GetAnalysisResult(inspection.Id);

            return new ServerResponse<Inspection>(inspection, "Saved");
        }

        public async Task<Inspection> Load(string id)
        {
            return await _session.LoadAsync<Inspection>(id);
        }

        public async Task<List<InspectionListItem>> LoadList(InspectionQueryParams prms)
        {
            var query = _session.Query<Inspection>().Include(c => c.SupplierId)
                .Where(c => c.CompanyId == prms.CompanyId);

            if (prms.Approved != null)
            {
                query = query.Where(c => c.AnalysisResult.Approved == prms.Approved);
            }

            var list = await query.OrderBy(c => c.InspectionDate).ThenBy(c=>c.Id)
                .Select(c => new InspectionListItem
                {
                    Approved = c.AnalysisResult.Approved,
                    Bags = c.Bags,
                    WeightKg = c.WeightKg,
                    Inspector = c.Inspector,
                    InspectionDate = c.InspectionDate,
                    Location = c.Location,
                    Id = c.Id,
                    LotNo = c.LotNo,
                    TruckPlate = c.TruckPlate,
                    SupplierId = c.SupplierId,
                    Kor = c.AnalysisResult.Kor,
                    Count = c.AnalysisResult.Count,
                    Moisture = c.AnalysisResult.Moisture,
                    RejectsPct = c.AnalysisResult.RejectsPct,
                    StockReferences = c.StockReferences,
                    StockAllocations = c.StockReferences.Count,
                    AvgBagWeightKg = c.AvgBagWeightKg,
                    Price = c.Price,
                    WarehouseId = c.WarehouseId,
                    Fiche = c.Fiche
                })
                .ToListAsync();

            foreach (var item in list)
            {
                item.UnallocatedBags = item.Bags - item.StockReferences.Sum(x => x.Bags);
                item.UnallocatedWeightKg = item.WeightKg - item.StockReferences.Sum(x => x.WeightKg);
            }

            var lookupCustomers = list.Select(x => x.SupplierId).Concat(list.Select(x => x.WarehouseId));
            var customers = await _session.Query<Customer>().Where(c => c.Id.In(lookupCustomers)).ToListAsync();

            foreach (var item in list)
            {
                item.SupplierName = customers.FirstOrDefault(x => x.Id == item.SupplierId)?.Name;
                item.WarehouseName = customers.FirstOrDefault(x => x.Id == item.WarehouseId)?.Name;
            }

            return list;
        }

        public async Task<List<AnalysisResult>> GetAnalysisResult(IEnumerable<string> inspectionIds)
        {
            var query = await _session.Query<Inspections_ByAnalysisResult.Result, Inspections_ByAnalysisResult>()
                .Where(c => c.InspectionId.In(inspectionIds))
                .ProjectInto<AnalysisResult>()
                .ToListAsync();

            return query;
        }

        public async Task<AnalysisResult> GetAnalysisResult(string inspectionId)
        {
            var result = await GetAnalysisResult(new[] {inspectionId});
            return result.Count == 0 ? new AnalysisResult() : result.First();
        }

        public async Task<ServerResponse> DeleteInspection(string id)
        {
            var inspection = await _session.LoadAsync<Inspection>(id);

            if (inspection.StockReferences.Any())
                throw new InvalidOperationException("Cannot delete an inspection if it has already been moved to stock");

            _session.Delete(inspection);
            await _session.SaveChangesAsync();

            var x = await _session.LoadAsync<Inspection>(id);
            Debug.Assert(x == null, "Should have delete inspection");

            return await Task.FromResult(new ServerResponse("Deleted inspection"));
        }
    }
}