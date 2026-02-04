using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;

namespace Warehouse.DataAccess.Services.ExcelExportService;

public class ExcelExportService : IExcelExportService
{
    private readonly ILogger<ExcelExportService> _logger;

    public ExcelExportService(ILogger<ExcelExportService> logger)
    {
        _logger = logger;
    }

    public Task<byte[]> ExportMonthlyItemsToExcelAsync(
        List<GetItemsWithVouchersOfMonthResult> items,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Excel file for month: {Month}, year: {Year}", month, year);

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add($"Items_{month:D2}_{year}");

            // Set RTL direction for the worksheet
            worksheet.View.RightToLeft = true;

            // Header row - Arabic labels
            worksheet.Cells[1, 1].Value = "كود الصنف";
            worksheet.Cells[1, 2].Value = "رقم القطعة";
            worksheet.Cells[1, 3].Value = "الوصف";
            worksheet.Cells[1, 4].Value = "الوحدة";
            worksheet.Cells[1, 5].Value = "التصنيف";
            worksheet.Cells[1, 6].Value = "القسم";
            worksheet.Cells[1, 7].Value = "كمية الوارد";
            worksheet.Cells[1, 8].Value = "كمية المنصرف";
            worksheet.Cells[1, 9].Value = "قيمة الوارد";
            worksheet.Cells[1, 10].Value = "قيمة المنصرف";

            // Header styling
            using (var range = worksheet.Cells[1, 1, 1, 10])
            {
                range.Style.Font.Bold = true;
                range.Style.Font.Size = 14;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            // Set header row height
            worksheet.Row(1).Height = 25;

            // Data rows
            int row = 2;
            foreach (var item in items)
            {
                worksheet.Cells[row, 1].Value = item.ItemCode;
                worksheet.Cells[row, 2].Value = item.PartNo ?? "";
                worksheet.Cells[row, 3].Value = item.Description;
                worksheet.Cells[row, 4].Value = item.Unit.ToString();
                worksheet.Cells[row, 5].Value = item.CategoryName;
                worksheet.Cells[row, 6].Value = item.SectionName;
                worksheet.Cells[row, 7].Value = item.VouchersTotalInQuantity;
                worksheet.Cells[row, 8].Value = item.VouchersTotalOutQuantity;
                worksheet.Cells[row, 9].Value = item.VouchersTotalInValue;
                worksheet.Cells[row, 10].Value = item.VouchersTotalOutValue;

                // Apply styling to data rows
                using (var range = worksheet.Cells[row, 1, row, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 14;
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                // Set row height
                worksheet.Row(row).Height = 22;

                row++;
            }

            // Add totals row if there's data
            if (row > 2)
            {
                worksheet.Cells[row, 1].Value = "الإجمالي";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 7].Formula = $"SUM(G2:G{row - 1})";
                worksheet.Cells[row, 8].Formula = $"SUM(H2:H{row - 1})";
                worksheet.Cells[row, 9].Formula = $"SUM(I2:I{row - 1})";
                worksheet.Cells[row, 10].Formula = $"SUM(J2:J{row - 1})";

                using (var range = worksheet.Cells[row, 1, row, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 13;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                // Set totals row height
                worksheet.Row(row).Height = 22;
            }

            // Auto-fit columns first
            if (row > 2)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }

            // Set minimum column widths for better readability
            worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 15); // كود الصنف
            worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 15); // رقم القطعة
            worksheet.Column(3).Width = Math.Max(worksheet.Column(3).Width, 30); // الوصف
            worksheet.Column(4).Width = Math.Max(worksheet.Column(4).Width, 12); // الوحدة
            worksheet.Column(5).Width = Math.Max(worksheet.Column(5).Width, 18); // التصنيف
            worksheet.Column(6).Width = Math.Max(worksheet.Column(6).Width, 18); // القسم
            worksheet.Column(7).Width = Math.Max(worksheet.Column(7).Width, 18); // كمية الوارد
            worksheet.Column(8).Width = Math.Max(worksheet.Column(8).Width, 18); // كمية المنصرف
            worksheet.Column(9).Width = Math.Max(worksheet.Column(9).Width, 18); // قيمة الوارد
            worksheet.Column(10).Width = Math.Max(worksheet.Column(10).Width, 18); // قيمة المنصرف

            // Format number columns with thousand separators
            for (int i = 2; i < row; i++)
            {
                worksheet.Cells[i, 7, i, 8].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[i, 9, i, 10].Style.Numberformat.Format = "#,##0.00";
            }

            // Format totals row numbers
            if (row > 2)
            {
                worksheet.Cells[row, 7, row, 8].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 9, row, 10].Style.Numberformat.Format = "#,##0.00";
            }

            _logger.LogInformation("Excel file generated successfully with {ItemCount} items", items.Count);

            return Task.FromResult(package.GetAsByteArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating Excel file");
            throw;
        }
    }

    public Task<byte[]> ExportAllItemsToExcelAsync(
        Dictionary<string, List<ItemExportData>> itemsBySections,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Excel file for all items with {SectionCount} sections", itemsBySections.Count);

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            foreach (var sectionEntry in itemsBySections)
            {
                var sectionName = sectionEntry.Key;
                var items = sectionEntry.Value;

                if (items.Count == 0)
                {
                    continue; // Skip sections with no items
                }

                // Create worksheet for this section
                var worksheetName = SanitizeSheetName(sectionName);
                var worksheet = package.Workbook.Worksheets.Add(worksheetName);

                // Set RTL direction for the worksheet
                worksheet.View.RightToLeft = true;

                // Title row with section name
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Value = sectionName;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells[1, 1].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells[1, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(217, 217, 217));
                worksheet.Cells[1, 1].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Cells[1, 1].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                worksheet.Row(1).Height = 30;

                // Header row - Arabic labels (row 2)
                worksheet.Cells[2, 1].Value = "الكود";
                worksheet.Cells[2, 2].Value = "الباركود";
                worksheet.Cells[2, 3].Value = "الصنف";
                worksheet.Cells[2, 4].Value = "الوحدة";
                worksheet.Cells[2, 5].Value = "الرصيد";
                worksheet.Cells[2, 6].Value = "السعر";

                // Header styling
                using (var range = worksheet.Cells[2, 1, 2, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Size = 14;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                // Set header row height
                worksheet.Row(2).Height = 25;

                // Data rows (starting from row 3)
                int row = 3;
                foreach (var item in items)
                {
                    worksheet.Cells[row, 1].Value = item.ItemCode;
                    worksheet.Cells[row, 2].Value = item.PartNo ?? "";
                    worksheet.Cells[row, 3].Value = item.Description;
                    worksheet.Cells[row, 4].Value = item.UnitArabic;
                    worksheet.Cells[row, 5].Value = item.AvailableQuantity;
                    worksheet.Cells[row, 6].Value = item.UnitPrice;

                    // Apply styling to data rows
                    using (var range = worksheet.Cells[row, 1, row, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Size = 14;
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                        range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    }

                    // Set row height
                    worksheet.Row(row).Height = 22;

                    row++;
                }

                // Auto-fit columns first
                if (row > 3)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                // Set minimum column widths for better readability
                worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 15); // كود
                worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 18); // الباركود
                worksheet.Column(3).Width = Math.Max(worksheet.Column(3).Width, 35); // الصنف
                worksheet.Column(4).Width = Math.Max(worksheet.Column(4).Width, 12); // الوحدة
                worksheet.Column(5).Width = Math.Max(worksheet.Column(5).Width, 12); // رصيد
                worksheet.Column(6).Width = Math.Max(worksheet.Column(6).Width, 16); // السعر

                // Format number columns with thousand separators
                for (int i = 3; i < row; i++)
                {
                    worksheet.Cells[i, 5].Style.Numberformat.Format = "#,##0";
                    worksheet.Cells[i, 6].Style.Numberformat.Format = "#,##0.00";
                }
            }

            _logger.LogInformation("Excel file generated successfully with {SectionCount} sections", package.Workbook.Worksheets.Count);

            return Task.FromResult(package.GetAsByteArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating Excel file for all items");
            throw;
        }
    }

    private static string SanitizeSheetName(string name)
    {
        // Excel sheet names cannot contain: \ / ? * [ ]
        // and must be 31 characters or less
        var invalidChars = new[] { '\\', '/', '?', '*', '[', ']', ':' };
        var sanitized = name;
        foreach (var c in invalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }

        if (sanitized.Length > 31)
        {
            sanitized = sanitized.Substring(0, 31);
        }

        return sanitized;
    }
}
