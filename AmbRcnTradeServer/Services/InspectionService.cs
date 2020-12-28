﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Constants;
using AmbRcnTradeServer.Models;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.InspectionModels;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Inspection = AmbRcnTradeServer.Models.InspectionModels.Inspection;

namespace AmbRcnTradeServer.Services
{
    public interface IInspectionService
    {
        Task<ServerResponse<Inspection>> Save(Inspection inspection);
        Task<Inspection> Load(string id);
        Task<List<InspectionListItem>> LoadList(InspectionQueryParams prms);
    }

    public class InspectionService : IInspectionService
    {
        private readonly IAsyncDocumentSession _session;

        public InspectionService(IAsyncDocumentSession session)
        {
            _session = session;
        }

        public async Task<ServerResponse<Inspection>> Save(Inspection inspection)
        {
            inspection.AnalysisResult = new Analysis()
            {
                Bags = 0,
                Count = CalculateAverageValue(inspection.Analyses, c => c.Count),
                Kor = CalculateAverageValue(inspection.Analyses, c => c.Kor),
                Moisture = CalculateAverageValue(inspection.Analyses, c => c.Moisture),
                Rejects = CalculateAverageValue(inspection.Analyses, c => c.Rejects),
                Sound = CalculateAverageValue(inspection.Analyses, c => c.Sound),
                Spotted = CalculateAverageValue(inspection.Analyses, c => c.Spotted),
            };

            await _session.StoreAsync(inspection);
            return new ServerResponse<Inspection>(inspection, "Saved");
        }

        public async Task<Inspection> Load(string id)
        {
            return await _session.LoadAsync<Inspection>(id);
        }

        private static double CalculateAverageValue(List<Analysis> list, Func<Analysis, double> field)
        {
            var bags = list.Where(c => c.Approved == Approval.Approved).Sum(c => c.Bags);
            return list.Where(c => c.Approved == Approval.Approved).Sum(c => c.Bags * field(c)) / bags;
        }

        public async Task<List<InspectionListItem>> LoadList(InspectionQueryParams prms)
        {
            var query = _session.Query<Inspection>().Include(c => c.SupplierId)
                .Where(c => c.CompanyId == prms.CompanyId);

            if (prms.Approved != null)
            {
                query = query.Where(c => c.Approved == prms.Approved);
            }

            var list = await query.OrderByDescending(c => c.InspectionDate)
                .Select(c => new InspectionListItem()
                {
                    Approved = c.Approved,
                    Bags = c.Bags,
                    Inspector = c.Inspector,
                    InspectionDate = c.InspectionDate,
                    Location = c.Location,
                    ApproxWeight = c.ApproxWeight,
                    InspectionId = c.Id,
                    LotNo = c.LotNo,
                    TruckPlate = c.TruckPlate,
                    SupplierId = c.SupplierId
                })
                .ToListAsync();

            var customers = await _session.Query<Customer>().Where(c => c.Id.In(list.Select(x => x.SupplierId))).ToListAsync();

            foreach (var item in list)
            {
                item.SupplierName = customers.FirstOrDefault(x => x.Id == item.SupplierId)?.Name;
            }

            return list;
        }
    }
}