using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AmberwoodCore.Exceptions;
using AmberwoodCore.Extensions;
using AmberwoodCore.Responses;
using AmbRcnTradeServer.Models.DictionaryModels;
using AmbRcnTradeServer.Models.DraftBillLadingModels;
using AmbRcnTradeServer.Models.VesselModels;
using GemBox.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IDraftBillLadingService
    {
        ExcelFile LoadTemplate(string templateFileName);
        Task<DraftBillLadingDataResponse> LoadData(string vesselId, string billLadingId);
        BillLadingCustomers GetBillLadingCustomers(DraftBillLadingDataResponse data);

        Task<ServerResponse> SaveWorkbook(string templateFileName, HttpResponse httpResponse, string vesselId, string billLadingId);
        ExcelFile FillTemplate(string templateFileName, DraftBillLadingDataResponse data);
    }

    public class DraftBillLadingService : IDraftBillLadingService
    {
        // private const string MaerskDraftBlTemplateBlXlsx = "Maersk Draft BL Template BL.xlsx";
        private readonly IBillLadingService _billLadingService;
        private readonly IAsyncDocumentSession _session;

        public DraftBillLadingService(IAsyncDocumentSession session, IBillLadingService billLadingService)
        {
            _session = session;
            _billLadingService = billLadingService;
        }

        public ExcelFile LoadTemplate(string templateFileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var currentPath = assembly.Location;
            var dllName = assembly.ManifestModule.Name;
            var dllPath = currentPath.Replace(dllName, "");
            var fileName = Path.Combine(dllPath, "ExcelTemplates", templateFileName);

            SpreadsheetInfo.SetLicense("FREE-LIMITED-KEY");

            var file = ExcelFile.Load(fileName);

            return file;
        }

        public async Task<DraftBillLadingDataResponse> LoadData(string vesselId, string billLadingId)
        {
            var vessel = await _session
                .Include<Vessel>(c => c.ForwardingAgentId)
                .LoadAsync<Vessel>(vesselId);

            var billOfLading = await _billLadingService.Load(billLadingId);
            var customerIds = new[]
            {
                vessel.ForwardingAgentId, vessel.ShippingCompanyId, billOfLading.ConsigneeId, billOfLading.ShipperId,
                billOfLading.NotifyParty1Id, billOfLading.NotifyParty2Id, billOfLading.DestinationAgentId
            };
            var customers = await _session.LoadListFromMultipleIdsAsync<Customer>(customerIds);
            var ports = await _session.LoadAsync<Port>(billOfLading.PortOfDestinationId);

            return new DraftBillLadingDataResponse {BillLadingDto = billOfLading, Vessel = vessel, Customers = customers, Ports = ports};
        }

        public ExcelFile FillTemplate(string templateFileName, DraftBillLadingDataResponse data)
        {
            var workbook = LoadTemplate(templateFileName);
            var worksheet = workbook.Worksheets[0];

            // var data = await LoadData(vesselId, billLadingId);
            var customers = GetBillLadingCustomers(data);

            SetCell(worksheet, customers.Shipper, c => c.CompanyName);
            SetCell(worksheet, customers.Shipper, c => c.Address1);
            SetCell(worksheet, customers.Shipper, c => c.Address2);
            SetCell(worksheet, customers.Shipper, c => c.Address3);
            SetCell(worksheet, customers.Shipper, c => c.Address4);
            SetCell(worksheet, customers.Shipper, c => c.Address5);

            SetCell(worksheet, customers.Consignee, c => c.CompanyName);
            SetCell(worksheet, customers.Consignee, c => c.Address1);
            SetCell(worksheet, customers.Consignee, c => c.Address2);
            SetCell(worksheet, customers.Consignee, c => c.Address3);
            SetCell(worksheet, customers.Consignee, c => c.Address4);
            SetCell(worksheet, customers.Consignee, c => c.Address5);

            SetCell(worksheet, customers.NotifyParty1, c => c.CompanyName);
            SetCell(worksheet, customers.NotifyParty1, c => c.Address1);
            SetCell(worksheet, customers.NotifyParty1, c => c.Address2);
            SetCell(worksheet, customers.NotifyParty1, c => c.Address3);
            SetCell(worksheet, customers.NotifyParty1, c => c.Address4);
            SetCell(worksheet, customers.NotifyParty1, c => c.Address5);

            SetCell(worksheet, customers.NotifyParty2, c => c.CompanyName);
            SetCell(worksheet, customers.NotifyParty2, c => c.Address1);
            SetCell(worksheet, customers.NotifyParty2, c => c.Address2);
            SetCell(worksheet, customers.NotifyParty2, c => c.Address3);
            SetCell(worksheet, customers.NotifyParty2, c => c.Address4);
            SetCell(worksheet, customers.NotifyParty2, c => c.Address5);

            SetCell(worksheet, customers.ForwardingAgent, c => c.CompanyName);
            SetCell(worksheet, customers.ForwardingAgent, c => c.Address1);
            SetCell(worksheet, customers.ForwardingAgent, c => c.Address2);
            SetCell(worksheet, customers.ForwardingAgent, c => c.Address3);
            SetCell(worksheet, customers.ForwardingAgent, c => c.Address4);
            SetCell(worksheet, customers.ForwardingAgent, c => c.Address5);

            SetCell(worksheet, "BillLading.ShipperReference", data.BillLadingDto.ShipperReference);
            SetCell(worksheet, "BillLading.ConsigneeReference", data.BillLadingDto.ConsigneeReference);
            SetCell(worksheet, "BillLading.ForwarderReference", data.BillLadingDto.ForwarderReference);
            SetCell(worksheet, "Vessel.ServiceContract", data.Vessel.ServiceContract);
            SetCell(worksheet, "BillLading.DestinationAgentAddress", data.Customers.FirstOrDefault(c => c.Id == data.BillLadingDto.DestinationAgentId)?.CompanyName);
            SetCell(worksheet, "Vessel.VesselName", data.Vessel.VesselName);
            SetCell(worksheet, "Vessel.VoyageNumber", data.Vessel.VoyageNumber);
            SetCell(worksheet, "BillLading.PortOfDestinationName", data.BillLadingDto.PortOfDestinationName);
            SetCell(worksheet, "BillLading.OceanFreight", "");
            SetCell(worksheet, "BillLading.OceanFreightPaidBy", "");
            SetCell(worksheet, "BillLading.FreightOriginCharges", "");
            SetCell(worksheet, "BillLading.FreightOriginChargesPaidBy", "");
            SetCell(worksheet, "BillLading.FreightDestinationCharge", "");
            SetCell(worksheet, "BillLading.FreightDestinationChargePaidBy", "");

            return workbook;
        }

        public BillLadingCustomers GetBillLadingCustomers(DraftBillLadingDataResponse data)
        {
            var shipper = data.Customers.FirstOrDefault(c => c.Id == data.BillLadingDto.ShipperId);
            var consignee = data.Customers.FirstOrDefault(c => c.Id == data.BillLadingDto.ConsigneeId);
            var notifyParty1 = data.Customers.FirstOrDefault(c => c.Id == data.BillLadingDto.NotifyParty1Id);
            var notifyParty2 = data.Customers.FirstOrDefault(c => c.Id == data.BillLadingDto.NotifyParty2Id);
            var freightForwarder = data.Customers.FirstOrDefault(c => c.Id == data.Vessel.ForwardingAgentId);

            const string BILL_LADING = "BillLading";
            const string VESSEL = "Vessel";

            var billLadingCustomers = new BillLadingCustomers
            {
                Shipper = CreateCustomer(BILL_LADING, "Shipper", shipper),
                Consignee = CreateCustomer(BILL_LADING, "Consignee", consignee),
                NotifyParty1 = CreateCustomer(BILL_LADING, "NotifyParty1", notifyParty1),
                NotifyParty2 = CreateCustomer(BILL_LADING, "NotifyParty2", notifyParty2),
                ForwardingAgent = CreateCustomer(VESSEL, "ForwardingAgent", freightForwarder)
            };

            return billLadingCustomers;
        }

        public async Task<ServerResponse> SaveWorkbook(string templateFileName, HttpResponse httpResponse, string vesselId, string billLadingId)
        {
            var data = await LoadData(vesselId, billLadingId);

            var fileName = $"{data.Vessel.VesselName}-{data.Vessel.BookingNumber}.xlsx";

            var workbook = FillTemplate(templateFileName, data);

            workbook.Save(httpResponse, fileName);

            return await Task.FromResult(new ServerResponse($"Saved {fileName}"));
        }

        private static void SetCell(ExcelWorksheet worksheet, string key, string value)
        {
            if (!worksheet.Cells.FindText(key, true, false, out var row, out var column))
                throw new NotFoundException($"Cannot find key {key} in template worksheet");

            var cell = worksheet.Cells[row, column];
            cell.Value = value;
        }

        private static void SetCell(ExcelWorksheet worksheet, BlCustomer customers, Func<BlCustomer, ExcelCellData> field)
        {
            SetCell(worksheet, field(customers).Key, field(customers).Value);
        }

        private static BlCustomer CreateCustomer(string root, string customerType, Customer customer)
        {
            var prefix = $"{root}.{customerType}";

            ExcelCellData GetAddressLine(List<string> addresses, int index)
            {
                return index < addresses.Count ? new ExcelCellData($"{prefix}Address{index + 1}", addresses[index]) : null;
            }

            var spcCity = customer.Address.City.IsNotNullOrEmpty() ? " " : "";
            var spcState = customer.Address.State.IsNotNullOrEmpty() ? " " : "";

            var list = new List<string>
            {
                customer.Address.Street1.Trim(),
                customer.Address.Street2.Trim(),
                $"{customer.Address.City?.Trim()}{spcCity}{customer.Address.State?.Trim()}{spcState}{customer.Address.Country?.Trim()}".Trim(),
                customer.Reference.Trim(),
                customer.Email.Trim()
            };

            var addressList = list.Where(c => c.IsNotNullOrEmpty()).ToList();

            var blCustomer = new BlCustomer
            {
                CompanyName = new ExcelCellData($"{prefix}CompanyName", customer.CompanyName),
                Address1 = GetAddressLine(addressList, 0),
                Address2 = GetAddressLine(addressList, 1),
                Address3 = GetAddressLine(addressList, 2),
                Address4 = GetAddressLine(addressList, 3),
                Address5 = GetAddressLine(addressList, 4)
            };

            return blCustomer;
        }
    }
}