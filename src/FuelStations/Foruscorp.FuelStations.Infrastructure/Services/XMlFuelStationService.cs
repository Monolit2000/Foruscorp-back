using ClosedXML.Excel;
using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using Z.EntityFramework.Extensions;
using DocumentFormat.OpenXml.Office.PowerPoint.Y2021.M06.Main;

public class XMlFuelStationService : IXMlFuelStationService
{

    public async Task<List<XmlTaAndPetroStationInfoModel>> ParceTaAndPetroStationInfoFile(IFormFile file, CancellationToken cancellationToken = default)
    {

        try
        {
            var result = new List<XmlTaAndPetroStationInfoModel>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            foreach (var row in worksheet.RowsUsed().Skip(1).Take(worksheet.RowsUsed().Count() - 1))
            {
                var model = new XmlTaAndPetroStationInfoModel
                {
                    Brand = row.Cell(2).GetString(),
                    Id = row.Cell(3).GetString().Trim(),
                    State = row.Cell(4).GetString().Trim(),
                    City = row.Cell(5).GetString().Trim(),
                    Location = row.Cell(5).GetString().Trim(),
                    Directions = row.Cell(7).GetString().Trim(),
                    Address = row.Cell(8).GetString().Trim(),
                    Latitude = ParseDouble(row.Cell(13).GetString()),
                    Longitude = ParseDouble(row.Cell(14).GetString()),
                };
                result.Add(model);
            }
            return result;
        }
        catch (Exception ex)
        {
            return new List<XmlTaAndPetroStationInfoModel>();
        }

    }

    public async Task<List<XmlTaAndPetroStationParceModel>> ParceTaAndPetroStationFile(IFormFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new List<XmlTaAndPetroStationParceModel>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            foreach (var row in worksheet.RowsUsed().Skip(2).Take(worksheet.RowsUsed().Count() - 2))
            {
                var model = new XmlTaAndPetroStationParceModel
                {
                    Id = row.Cell(1).GetString().Trim(),
                    ECSCost = ParseDouble(row.Cell(2).GetString()),
                    Discount = ParseDouble(row.Cell(3).GetString()),
                    TravelCenter = row.Cell(4).GetString().Trim(),
                    State = row.Cell(5).GetString().Trim(),
                    Price = ParseDouble(row.Cell(6).GetString()),
                };
                result.Add(model);
            }
            return result;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }



    public async Task<List<XmlLoversStationParceModel>> ParceLoversExcelFile(IFormFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new List<XmlLoversStationParceModel>();

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            foreach (var row in worksheet.RowsUsed().Skip(1).Take(worksheet.RowsUsed().Count() - 2))
            {
                var model = new XmlLoversStationParceModel
                {
                    Id = row.Cell(1).GetString().Trim(),
                    ECSCost = ParseDecimal(row.Cell(2).GetString()),
                    Discount = ParseDecimal(row.Cell(3).GetString()),
                    City = row.Cell(4).GetString().Trim(),
                    State = row.Cell(5).GetString().Trim(),
                    PumpPrice = ParseDecimal(row.Cell(6).GetString()),
                    EffectiveDate = row.Cell(7).GetString().Trim()
                };
                result.Add(model);
            }
            return result;
        }
        catch (Exception ex)
        {
            return new List<XmlLoversStationParceModel>(); 
        }
    }
    private decimal ParseDecimal(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;

        input = input.Replace(',', '.');
        if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return Math.Round(value, 3);
        }

        return 0;
    }


    private double ParseDouble(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return 0;

        input = input.Replace(',', '.');

        if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            return Math.Round(value, 3);
        }

        return 0;
    }
}
