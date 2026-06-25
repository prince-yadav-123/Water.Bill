using ClosedXML.Excel;
using Water.Bill.Application.Interfaces;
using Water.Bill.Application.Models.Excel;

namespace Water.Bill.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    public byte[] Export<T>(ExcelExportRequest<T> request)
    {
        using var workbook = new XLWorkbook();
        var sheetName = SanitizeSheetName(request.SheetName);
        var worksheet = workbook.Worksheets.Add(sheetName);

        worksheet.Cell(1, 1).Value = "S.No.";
        worksheet.Cell(1, 1).Style.Font.Bold = true;

        for (var index = 0; index < request.Columns.Count; index++)
        {
            var cell = worksheet.Cell(1, index + 2);
            cell.Value = request.Columns[index].Header;
            cell.Style.Font.Bold = true;
        }

        for (var rowIndex = 0; rowIndex < request.Rows.Count; rowIndex++)
        {
            var rowNumber = rowIndex + 2;
            worksheet.Cell(rowNumber, 1).Value = rowIndex + 1;

            for (var colIndex = 0; colIndex < request.Columns.Count; colIndex++)
            {
                var column = request.Columns[colIndex];
                var cell = worksheet.Cell(rowNumber, colIndex + 2);
                WriteCellValue(cell, column.ValueFactory(request.Rows[rowIndex]));

                if (!string.IsNullOrWhiteSpace(column.NumberFormat))
                    cell.Style.NumberFormat.Format = column.NumberFormat;
            }
        }

        var headerRange = worksheet.Range(1, 1, 1, request.Columns.Count + 1);
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#DCE9F8");
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

        worksheet.SheetView.FreezeRows(1);
        worksheet.Columns().AdjustToContents();
        worksheet.Column(1).Width = 10;

        for (var index = 0; index < request.Columns.Count; index++)
        {
            if (request.Columns[index].Width is double width)
                worksheet.Column(index + 2).Width = width;
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = string.Empty;
                break;
            case DateTime dateTime:
                cell.Value = dateTime;
                break;
            case DateTimeOffset dateTimeOffset:
                cell.Value = dateTimeOffset.LocalDateTime;
                break;
            case bool boolValue:
                cell.Value = boolValue ? "Yes" : "No";
                break;
            case byte byteValue:
                cell.Value = byteValue;
                break;
            case sbyte sbyteValue:
                cell.Value = sbyteValue;
                break;
            case short shortValue:
                cell.Value = shortValue;
                break;
            case ushort ushortValue:
                cell.Value = ushortValue;
                break;
            case int intValue:
                cell.Value = intValue;
                break;
            case uint uintValue:
                cell.Value = uintValue;
                break;
            case long longValue:
                cell.Value = longValue;
                break;
            case ulong ulongValue:
                cell.Value = ulongValue.ToString();
                break;
            case float floatValue:
                cell.Value = floatValue;
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static string SanitizeSheetName(string? name)
    {
        const string fallback = "Sheet1";
        if (string.IsNullOrWhiteSpace(name))
            return fallback;

        var invalidChars = Path.GetInvalidFileNameChars().Concat(['[', ']', '*', '?', '/', '\\']).Distinct().ToHashSet();
        var sanitized = new string(name.Trim().Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());
        if (sanitized.Length > 31)
            sanitized = sanitized[..31];

        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }
}
