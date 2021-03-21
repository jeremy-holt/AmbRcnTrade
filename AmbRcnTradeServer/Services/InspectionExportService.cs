using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AmbRcnTradeServer.Models.InspectionModels;
using GemBox.Spreadsheet;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;

namespace AmbRcnTradeServer.Services
{
    public interface IInspectionExportService
    {
        ExcelFile LoadTemplate(string templateFileName);
        ExcelFile GetWorkbook(string templateFileName, List<InspectionListItem> inspections);
        Task<InspectionExportResponse> SaveWorkbook(ExcelFile workbook);
    }

    public class InspectionExportService : IInspectionExportService
    {
        public ExcelFile LoadTemplate(string templateFileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var currentPath = assembly.Location;
            var dllName = assembly.ManifestModule.Name;
            var dllPath = currentPath.Replace(dllName, "");
            var fileName = Path.Combine(dllPath, "ExcelTemplates", templateFileName);

            SpreadsheetInfo.SetLicense("SN-2021Jan16-wV4XsTHbtRl0O7fSRvJOlkVTICh9RbsKOOEcmc4BfETFCeRF/tPuJD0ihDmodWDlwFpItYvuXe44dKQokVkj3uXPsnQ==A");

            var file = ExcelFile.Load(fileName);

            return file;
        }

        public ExcelFile GetWorkbook(string templateFileName, List<InspectionListItem> inspections)
        {
            var workbook = LoadTemplate(templateFileName);
            var worksheet = workbook.Worksheets[0];

            const int row = 7;
            worksheet.Rows.InsertCopy(row + 1, inspections.Count - 1, worksheet.Rows[row]);

            for (var i = 0; i < inspections.Count; i++)
            {
                var currentRow = worksheet.Rows[row + i];
                var inspection = inspections[i];
                currentRow.Cells[0].SetValue($"Inspection {ExtractIdNumber(inspection.Id)}");
                currentRow.Cells[1].SetValue($"{inspection.InspectionDate:dd/MM/yyyy}");
                currentRow.Cells[2].SetValue(inspection.WarehouseName);
                currentRow.Cells[3].SetValue(inspection.BuyerName);
                currentRow.Cells[4].SetValue(inspection.SupplierName);
                currentRow.Cells[5].SetValue(inspection.Fiche);
                currentRow.Cells[6].SetValue(inspection.TruckPlate);
                currentRow.Cells[7].SetValue(inspection.Price);
                currentRow.Cells[8].SetValue(inspection.Bags);
                currentRow.Cells[9].SetValue(inspection.WeightKg);
                
                currentRow.Cells[12].SetValue(Math.Round(inspection.Kor, 2));
                currentRow.Cells[13].SetValue(inspection.Count);
                currentRow.Cells[14].SetValue(inspection.Moisture/100);
                currentRow.Cells[15].SetValue(inspection.RejectsPct);
            }

            return workbook;
        }

        public async Task<InspectionExportResponse> SaveWorkbook(ExcelFile workbook)
        {
            var fileName = $"Inspections {DateTime.Today:D}";

            var options = SaveOptions.XlsxDefault;
            await using var ms = new MemoryStream();
            workbook.Save(ms, options);

            return new InspectionExportResponse(ms.ToArray(), options.ContentType, fileName);
        }

        private string ExtractIdNumber(string id)
        {
            return id.Split("/")[1].Split("-")[0];
        }
    }

    public class InspectionExportResponse
    {
        public InspectionExportResponse(byte[] fileContents, string contentType, string fileName)
        {
            FileContents = fileContents;
            ContentType = contentType;
            FileName = fileName;
        }

        public byte[] FileContents { get; }
        public string ContentType { get; }
        public string FileName { get; }
    }
}