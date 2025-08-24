using ClosedXML.Excel;
using ExcelDataReader;
using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Data;
using System.Text;

namespace Foruscorp.FuelStations.Infrastructure.Services
{
    public class ExcelConverterService : IExcelConverterService
    {
        private readonly ILogger<ExcelConverterService> _logger;

        public ExcelConverterService(ILogger<ExcelConverterService> logger)
        {
            _logger = logger;
        }

        public async Task<IFormFile> ConvertXlsToXlsxAsync(IFormFile xlsFile, CancellationToken cancellationToken = default)
        {
            if (xlsFile == null || xlsFile.Length == 0)
                throw new ArgumentException("XLS file is empty or null", nameof(xlsFile));

            if (!IsXlsFile(xlsFile.FileName))
                throw new ArgumentException("File is not a XLS file", nameof(xlsFile));

            try
            {
                _logger.LogDebug("Starting XLS to XLSX conversion for file: {FileName}", xlsFile.FileName);
                
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                
                using var xlsStream = xlsFile.OpenReadStream();
                
                using var reader = ExcelReaderFactory.CreateReader(xlsStream);
                _logger.LogDebug("Created ExcelDataReader");
                
                var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = false 
                    }
                });
                _logger.LogDebug("Successfully read XLS data. Tables count: {TablesCount}", dataSet.Tables.Count);
                
                using var xlsxWorkbook = new XLWorkbook();
                
                for (int tableIndex = 0; tableIndex < dataSet.Tables.Count; tableIndex++)
                {
                    var dataTable = dataSet.Tables[tableIndex];
                    var sheetName = !string.IsNullOrEmpty(dataTable.TableName) ? dataTable.TableName : $"Sheet{tableIndex + 1}";
                    
                    var worksheet = xlsxWorkbook.Worksheets.Add(sheetName);
                    
                    for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                    {
                        var dataRow = dataTable.Rows[rowIndex];
                        for (int colIndex = 0; colIndex < dataTable.Columns.Count; colIndex++)
                        {
                            var cellValue = dataRow[colIndex];
                            if (cellValue != null && cellValue != DBNull.Value)
                            {
                                worksheet.Cell(rowIndex + 1, colIndex + 1).Value = cellValue.ToString();
                            }
                        }
                    }
                    
                }
                
                byte[] xlsxBytes;
                using (var xlsxStream = new MemoryStream())
                {
                    xlsxWorkbook.SaveAs(xlsxStream);
                    xlsxBytes = xlsxStream.ToArray();
                }
                
                var xlsxFileName = Path.ChangeExtension(xlsFile.FileName, ".xlsx");
                var convertedFile = new FormFileWrapper(xlsxBytes, xlsxFileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                return convertedFile;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Attempting fallback conversion using NPOI for file: {FileName}", xlsFile.FileName);
                return await ConvertXlsToXlsxUsingNpoiFallback(xlsFile, cancellationToken);
            }
        }

        private async Task<IFormFile> ConvertXlsToXlsxUsingNpoiFallback(IFormFile xlsFile, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting NPOI fallback conversion for file: {FileName}", xlsFile.FileName);
                
                byte[] xlsBytes;
                using (var tempStream = new MemoryStream())
                {
                    await xlsFile.CopyToAsync(tempStream, cancellationToken);
                    xlsBytes = tempStream.ToArray();
                }

                using var xlsStream = new MemoryStream(xlsBytes);
                using var xlsWorkbook = new HSSFWorkbook(xlsStream);
                using var xlsxWorkbook = new XSSFWorkbook();

                for (int sheetIndex = 0; sheetIndex < xlsWorkbook.NumberOfSheets; sheetIndex++)
                {
                    var sourceSheet = xlsWorkbook.GetSheetAt(sheetIndex);
                    var targetSheet = xlsxWorkbook.CreateSheet(sourceSheet.SheetName);
                    CopySheet(sourceSheet, targetSheet, xlsxWorkbook);
                }

                // Сохраняем в byte array
                byte[] xlsxBytes;
                using (var xlsxStream = new MemoryStream())
                {
                    xlsxWorkbook.Write(xlsxStream, false);
                    xlsxBytes = xlsxStream.ToArray();
                }
                
                var xlsxFileName = Path.ChangeExtension(xlsFile.FileName, ".xlsx");
                var convertedFile = new FormFileWrapper(xlsxBytes, xlsxFileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                _logger.LogInformation("Successfully converted XLS to XLSX using NPOI fallback: {OriginalFile}", xlsFile.FileName);
                return convertedFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NPOI fallback conversion also failed for file: {FileName}", xlsFile.FileName);
                throw new InvalidOperationException($"Both ExcelDataReader and NPOI failed to convert XLS to XLSX: {ex.Message}", ex);
            }
        }

        public bool IsXlsFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            return fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase) &&
                   !fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
        }

        private void CopySheet(ISheet sourceSheet, ISheet targetSheet, IWorkbook targetWorkbook)
        {
            for (int rowIndex = 0; rowIndex <= sourceSheet.LastRowNum; rowIndex++)
            {
                var sourceRow = sourceSheet.GetRow(rowIndex);
                if (sourceRow == null) continue;

                var targetRow = targetSheet.CreateRow(rowIndex);

                for (int cellIndex = 0; cellIndex < sourceRow.LastCellNum; cellIndex++)
                {
                    var sourceCell = sourceRow.GetCell(cellIndex);
                    if (sourceCell == null) continue;

                    var targetCell = targetRow.CreateCell(cellIndex);

                    switch (sourceCell.CellType)
                    {
                        case CellType.String:
                            targetCell.SetCellValue(sourceCell.StringCellValue);
                            break;
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(sourceCell))
                            {
                                var dateValue = sourceCell.DateCellValue;
                                if (dateValue.HasValue)
                                {
                                    targetCell.SetCellValue(dateValue.Value);
                                }
                                else
                                {
                                    targetCell.SetCellValue("");
                                }
                            }
                            else
                            {
                                targetCell.SetCellValue(sourceCell.NumericCellValue);
                            }
                            break;
                        case CellType.Boolean:
                            targetCell.SetCellValue(sourceCell.BooleanCellValue);
                            break;
                        case CellType.Formula:
                            targetCell.SetCellFormula(sourceCell.CellFormula);
                            break;
                        case CellType.Blank:
                            targetCell.SetCellValue("");
                            break;
                        default:
                            targetCell.SetCellValue(sourceCell.ToString());
                            break;
                    }
                }
            }
        }
    }

    internal class FormFileWrapper : IFormFile
    {
        private readonly byte[] _content;

        public FormFileWrapper(byte[] content, string fileName, string contentType)
        {
            _content = content;
            FileName = fileName;
            Name = fileName;
            ContentType = contentType;
            Length = content.Length;
        }

        public string ContentType { get; }
        public string ContentDisposition => $"form-data; name=\"{Name}\"; filename=\"{FileName}\"";
        public IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public long Length { get; }
        public string Name { get; }
        public string FileName { get; }

        public Stream OpenReadStream()
        {
            return new MemoryStream(_content);
        }

        public void CopyTo(Stream target)
        {
            using var stream = OpenReadStream();
            stream.CopyTo(target);
        }

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            using var stream = OpenReadStream();
            await stream.CopyToAsync(target, cancellationToken);
        }
    }
}
