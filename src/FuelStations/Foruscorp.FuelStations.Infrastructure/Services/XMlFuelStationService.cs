using ClosedXML.Excel;
using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;
using Microsoft.AspNetCore.Http;
using System.Globalization;

public class XMlFuelStationService : IXMlFuelStationService
{
    public async Task<List<XmlLoversStationParceModel>> ParceLoversExcelFile(IFormFile file, CancellationToken cancellationToken = default)
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
                StoreId = row.Cell(1).GetString().Trim(),
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
}
