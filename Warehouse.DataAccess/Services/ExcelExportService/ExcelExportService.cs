using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Warehouse.Entities.DTO.Items.Export;
using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;
using Warehouse.Entities.DTO.ItemVoucher.ExportVouchers;
using Warehouse.Entities.Utilities.Enums;

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
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add($"Items_{month:D2}_{year}");

            ws.RightToLeft = true;

            // Headers
            string[] headers = ["كود الصنف", "رقم القطعة", "الوصف", "الوحدة", "التصنيف", "القسم",
                                 "كمية الوارد", "كمية المنصرف", "قيمة الوارد", "قيمة المنصرف"];
            for (int col = 1; col <= headers.Length; col++)
                ws.Cell(1, col).Value = headers[col - 1];

            var headerRange = ws.Range(1, 1, 1, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 16;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Row(1).Height = 30;

            // Data rows
            int row = 2;
            foreach (var item in items)
            {
                ws.Cell(row, 1).Value = item.ItemCode;
                ws.Cell(row, 2).Value = item.PartNo ?? "";
                ws.Cell(row, 3).Value = item.Description;
                ws.Cell(row, 4).Value = TranslateUnitToArabic(item.Unit);
                ws.Cell(row, 5).Value = item.CategoryName;
                ws.Cell(row, 6).Value = item.SectionName;
                ws.Cell(row, 7).Value = item.VouchersTotalInQuantity;
                ws.Cell(row, 8).Value = item.VouchersTotalOutQuantity;
                ws.Cell(row, 9).Value = item.VouchersTotalInValue;
                ws.Cell(row, 10).Value = item.VouchersTotalOutValue;

                var dataRange = ws.Range(row, 1, row, 10);
                dataRange.Style.Font.Bold = true;
                dataRange.Style.Font.FontSize = 14;
                dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Row(row).Height = 22;

                row++;
            }

            // Totals row
            if (row > 2)
            {
                ws.Cell(row, 1).Value = "الإجمالي";
                ws.Cell(row, 7).FormulaA1 = $"SUM(G2:G{row - 1})";
                ws.Cell(row, 8).FormulaA1 = $"SUM(H2:H{row - 1})";
                ws.Cell(row, 9).FormulaA1 = $"SUM(I2:I{row - 1})";
                ws.Cell(row, 10).FormulaA1 = $"SUM(J2:J{row - 1})";

                var totalsRange = ws.Range(row, 1, row, 10);
                totalsRange.Style.Font.Bold = true;
                totalsRange.Style.Font.FontSize = 13;
                totalsRange.Style.Fill.BackgroundColor = XLColor.LightYellow;
                totalsRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                totalsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                totalsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Row(row).Height = 22;

                // Number formats
                ws.Range(2, 7, row, 8).Style.NumberFormat.Format = "#,##0";
                ws.Range(2, 9, row, 10).Style.NumberFormat.Format = "#,##0.00";
            }

            // Column widths
            ws.Columns().AdjustToContents();
            double[] minWidths = [15, 15, 30, 12, 18, 18, 18, 18, 18, 18];
            for (int col = 1; col <= minWidths.Length; col++)
                if (ws.Column(col).Width < minWidths[col - 1])
                    ws.Column(col).Width = minWidths[col - 1];

            _logger.LogInformation("Excel file generated successfully with {ItemCount} items", items.Count);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
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
            using var workbook = new XLWorkbook();

            foreach (var sectionEntry in itemsBySections)
            {
                var sectionName = sectionEntry.Key;
                var items = sectionEntry.Value;

                if (items.Count == 0) continue;

                var ws = workbook.Worksheets.Add(SanitizeSheetName(sectionName));
                ws.RightToLeft = true;

                // Title row
                ws.Range(1, 1, 1, 6).Merge();
                ws.Cell(1, 1).Value = sectionName;
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(1, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                ws.Range(1, 1, 1, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Row(1).Height = 30;

                // Headers
                string[] headers = ["الكود", "الباركود", "الصنف", "الوحدة", "الرصيد", "السعر"];
                for (int col = 1; col <= headers.Length; col++)
                    ws.Cell(2, col).Value = headers[col - 1];

                var headerRange = ws.Range(2, 1, 2, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.FontSize = 14;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                ws.Row(2).Height = 25;

                // Data rows
                int row = 3;
                foreach (var item in items)
                {
                    ws.Cell(row, 1).Value = item.ItemCode;
                    ws.Cell(row, 2).Value = item.PartNo ?? "";
                    ws.Cell(row, 3).Value = item.Description;
                    ws.Cell(row, 4).Value = TranslateUnitToArabic(item.Unit);
                    ws.Cell(row, 5).Value = item.AvailableQuantity;
                    ws.Cell(row, 6).Value = item.UnitPrice;

                    var dataRange = ws.Range(row, 1, row, 6);
                    dataRange.Style.Font.Bold = true;
                    dataRange.Style.Font.FontSize = 14;
                    dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Row(row).Height = 22;

                    row++;
                }

                // Number formats
                if (row > 3)
                {
                    ws.Range(3, 5, row - 1, 5).Style.NumberFormat.Format = "#,##0";
                    ws.Range(3, 6, row - 1, 6).Style.NumberFormat.Format = "#,##0.00";
                }

                // Column widths
                ws.Columns().AdjustToContents();
                double[] minWidths = [15, 18, 35, 12, 12, 16];
                for (int col = 1; col <= minWidths.Length; col++)
                    if (ws.Column(col).Width < minWidths[col - 1])
                        ws.Column(col).Width = minWidths[col - 1];
            }

            _logger.LogInformation("Excel file generated successfully with {SectionCount} sections",
                workbook.Worksheets.Count);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating Excel file for all items");
            throw;
        }
    }

    public Task<byte[]> ExportVouchersToExcelAsync(
        List<VoucherExportData> vouchers,
        VoucherType voucherType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Excel file for {VoucherType} vouchers with {VoucherCount} vouchers",
            voucherType, vouchers.Count);

        try
        {
            using var workbook = new XLWorkbook();

            var vouchersByCode = vouchers
                .OrderBy(v => v.VoucherCode.Length)
                .ThenBy(v => v.VoucherCode)
                .ToList();

            string titlePrefix = voucherType == VoucherType.In ? "إذن وارد" : "إذن صرف مادة";

            foreach (var voucher in vouchersByCode)
            {
                var ws = workbook.Worksheets.Add(SanitizeSheetName(voucher.VoucherCode));
                ws.RightToLeft = true;

                int currentRow = 1;

                // Header: Date | Title | Voucher Number
                ws.Range(currentRow, 1, currentRow, 2).Merge();
                ws.Cell(currentRow, 1).Value = $"التاريخ: {voucher.VoucherDate:dd/MM/yyyy}";
                StyleHeaderCell(ws.Cell(currentRow, 1));

                ws.Range(currentRow, 3, currentRow, 4).Merge();
                ws.Cell(currentRow, 3).Value = titlePrefix;
                StyleHeaderCell(ws.Cell(currentRow, 3));

                ws.Range(currentRow, 5, currentRow, 6).Merge();
                ws.Cell(currentRow, 5).Value = $"رقم: {voucher.VoucherCode}";
                StyleHeaderCell(ws.Cell(currentRow, 5));

                ws.Row(currentRow).Height = 25;
                currentRow++;

                // Table headers (2 rows: merged + sub-headers)
                ws.Range(currentRow, 1, currentRow + 1, 1).Merge();
                ws.Cell(currentRow, 1).Value = "م";

                ws.Range(currentRow, 2, currentRow + 1, 2).Merge();
                ws.Cell(currentRow, 2).Value = "كود المادة";

                ws.Range(currentRow, 3, currentRow + 1, 3).Merge();
                ws.Cell(currentRow, 3).Value = "اسم المادة ومواصفاتها";

                ws.Range(currentRow, 4, currentRow, 5).Merge();
                ws.Cell(currentRow, 4).Value = "الكمية";

                ws.Range(currentRow, 6, currentRow + 1, 6).Merge();
                ws.Cell(currentRow, 6).Value = "الشعبة";

                ws.Cell(currentRow + 1, 4).Value = "العدد";
                ws.Cell(currentRow + 1, 5).Value = "الوحدة";

                var tableHeaderRange = ws.Range(currentRow, 1, currentRow + 1, 6);
                tableHeaderRange.Style.Font.Bold = true;
                tableHeaderRange.Style.Font.FontSize = 12;
                tableHeaderRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                tableHeaderRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                tableHeaderRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                tableHeaderRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                tableHeaderRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                ws.Row(currentRow).Height = 22;
                ws.Row(currentRow + 1).Height = 22;
                currentRow += 2;

                // Data rows
                int itemNumber = 1;
                foreach (var item in voucher.Items)
                {
                    ws.Cell(currentRow, 1).Value = itemNumber;
                    ws.Cell(currentRow, 2).Value = item.ItemPartNo;
                    ws.Cell(currentRow, 3).Value = item.Description;
                    ws.Cell(currentRow, 4).Value = item.Quantity;
                    ws.Cell(currentRow, 5).Value = TranslateUnitToArabic(item.Unit);
                    ws.Cell(currentRow, 6).Value = item.SectionName;

                    var dataRange = ws.Range(currentRow, 1, currentRow, 6);
                    dataRange.Style.Font.Bold = true;
                    dataRange.Style.Font.FontSize = 11;
                    dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    ws.Row(currentRow).Height = 25;

                    currentRow++;
                    itemNumber++;
                }

                // Column widths
                ws.Column(1).Width = 8;
                ws.Column(2).Width = 20;
                ws.Column(3).Width = 40;
                ws.Column(4).Width = 12;
                ws.Column(5).Width = 12;
                ws.Column(6).Width = 18;

                // Page setup (A4 print)
                ws.PageSetup.PaperSize = XLPaperSize.A4Paper;
                ws.PageSetup.PageOrientation = XLPageOrientation.Portrait;
                ws.PageSetup.FitToPages(1, 0);
                ws.PageSetup.Margins.Left = 0.5;
                ws.PageSetup.Margins.Right = 0.5;
                ws.PageSetup.Margins.Top = 0.75;
                ws.PageSetup.Margins.Bottom = 0.75;
                ws.PageSetup.Margins.Header = 0.3;
                ws.PageSetup.Margins.Footer = 0.3;
                ws.PageSetup.CenterHorizontally = true;
            }

            _logger.LogInformation("Excel file generated successfully with {SheetCount} sheets",
                workbook.Worksheets.Count);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating vouchers Excel file");
            throw;
        }
    }

    private static void StyleHeaderCell(IXLCell cell)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Font.FontSize = 14;
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }

    private static string SanitizeSheetName(string name)
    {
        var invalidChars = new[] { '\\', '/', '?', '*', '[', ']', ':' };
        var sanitized = name;
        foreach (var c in invalidChars)
            sanitized = sanitized.Replace(c, '_');

        return sanitized.Length > 31 ? sanitized[..31] : sanitized;
    }

    private static string TranslateUnitToArabic(UnitOfMeasure unit) => unit switch
    {
        UnitOfMeasure.Piece => "عدد",
        UnitOfMeasure.Kilogram => "كيلوجرام",
        UnitOfMeasure.Meter => "متر",
        UnitOfMeasure.Liter => "لتر",
        UnitOfMeasure.Box => "صندوق",
        UnitOfMeasure.Carton => "كرتون",
        _ => unit.ToString()
    };
}