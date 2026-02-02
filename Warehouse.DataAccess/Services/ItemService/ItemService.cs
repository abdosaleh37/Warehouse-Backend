using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using Warehouse.DataAccess.ApplicationDbContext;
using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.Delete;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth;
using Warehouse.Entities.DTO.Items.Search;
using Warehouse.Entities.DTO.Items.Update;
using Warehouse.Entities.Entities;
using Warehouse.Entities.Shared.ResponseHandling;
using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.DataAccess.Services.ItemService;

public class ItemService : IItemService
{
    private readonly WarehouseDbContext _context;
    private readonly IMapper _mapper;
    private readonly ResponseHandler _responseHandler;
    private readonly ILogger<ItemService> _logger;

    public ItemService(WarehouseDbContext context,
        IMapper mapper,
        ResponseHandler responseHandler,
        ILogger<ItemService> logger)
    {
        _context = context;
        _mapper = mapper;
        _responseHandler = responseHandler;
        _logger = logger;
    }

    public async Task<Response<GetItemsOfSectionResponse>> GetItemsofSectionAsync(
        Guid userId,
        GetItemsOfSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting Items of section: {SectionId}", request.SectionId);

        try
        {
            var section = await _context.Sections
                .FirstOrDefaultAsync(s => s.Id == request.SectionId
                    && s.Category.Warehouse.UserId == userId, cancellationToken);

            if (section == null)
            {
                _logger.LogWarning("Section: {SectionId} not found.", request.SectionId);
                return _responseHandler.NotFound<GetItemsOfSectionResponse>("Section not found");
            }

            var search = request.SearchString?.Trim();

            var query = _context.Items
                .AsNoTracking()
                .Where(i => i.SectionId == request.SectionId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i =>
                    (i.ItemCode != null && EF.Functions.Like(i.ItemCode, $"%{search}%")) ||
                    (i.PartNo != null && EF.Functions.Like(i.PartNo, $"%{search}%")) ||
                    (i.Description != null && EF.Functions.Like(i.Description, $"%{search}%"))
                );
            }

            var items = await query
                .OrderBy(i => i.ItemCode.Length)
                    .ThenBy(i => i.ItemCode)
                        .ThenBy(i => i.CreatedAt)
                .Select(i => new
                {
                    Item = i,
                    NetQuantity = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => v.InQuantity - v.OutQuantity) : 0,
                    NetValue = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice) : 0m
                })
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                _logger.LogInformation("No items found in section: {SectionId}", request.SectionId);
                return _responseHandler.Success(new GetItemsOfSectionResponse
                {
                    SectionId = section.Id,
                    SectionName = section.Name,
                    Items = new List<GetItemsOfSectionResult>(),
                    TotalItemsCount = 0
                }, "No items found in this section.");

            }

            var itemResults = items.Select(x =>
            {
                var mapped = _mapper.Map<GetItemsOfSectionResult>(x.Item);
                mapped.AvailableQuantity = x.Item.OpeningQuantity + x.NetQuantity;
                mapped.AvailableValue = (x.Item.OpeningUnitPrice * x.Item.OpeningQuantity) + x.NetValue;
                mapped.OpeningDate = DateTime.SpecifyKind(x.Item.OpeningDate, DateTimeKind.Utc);
                mapped.CreatedAt = DateTime.SpecifyKind(x.Item.CreatedAt, DateTimeKind.Utc);
                return mapped;
            }).ToList();

            var responseData = new GetItemsOfSectionResponse
            {
                SectionId = section.Id,
                SectionName = section.Name,
                Items = itemResults,
                TotalItemsCount = itemResults.Count
            };

            _logger.LogInformation("Retrieved {ItemCount} items from section: {SectionId}",
                itemResults.Count, request.SectionId);
            return _responseHandler.Success(responseData, "Items retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetItemsofSectionAsync cancelled for Section: {SectionId}", request.SectionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving items for Section: {SectionId}", request.SectionId);
            return _responseHandler.InternalServerError<GetItemsOfSectionResponse>("An error occurred while retrieving items.");
        }
    }

    public async Task<Response<GetItemByIdResponse>> GetByIdAsync(
        Guid userId,
        GetItemByIdRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting item by Id: {ItemId}", request.Id);

        try
        {
            var itemResult = await _context.Items
                .AsNoTracking()
                .Where(i => i.Id == request.Id
                    && i.Section.Category.Warehouse.UserId == userId)
                .Select(i => new
                {
                    Item = i,
                    SectionName = i.Section.Name,
                    NetQuantity = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => v.InQuantity - v.OutQuantity) : 0,
                    NetValue = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice) : 0m
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (itemResult == null)
            {
                _logger.LogWarning("Item with Id: {ItemId} not found.", request.Id);
                return _responseHandler.NotFound<GetItemByIdResponse>("Item not found");
            }

            var response = _mapper.Map<GetItemByIdResponse>(itemResult.Item);
            response.SectionName = itemResult.SectionName;
            response.AvailableQuantity = itemResult.Item.OpeningQuantity + itemResult.NetQuantity;
            response.AvailableValue = (itemResult.Item.OpeningUnitPrice * itemResult.Item.OpeningQuantity) + itemResult.NetValue;
            response.OpeningDate = DateTime.SpecifyKind(itemResult.Item.OpeningDate, DateTimeKind.Utc);
            response.CreatedAt = DateTime.SpecifyKind(itemResult.Item.CreatedAt, DateTimeKind.Utc);

            _logger.LogInformation("Item with Id: {ItemId} retrieved successfully.", request.Id);
            return _responseHandler.Success(response, "Item retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetByIdAsync cancelled for Item: {ItemId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving item with Id: {ItemId}", request.Id);
            return _responseHandler.InternalServerError<GetItemByIdResponse>("An error occurred while retrieving the item.");
        }
    }

    public async Task<Response<GetItemsWithVouchersOfMonthResponse>> GetItemsWithVouchersOfMonthAsync(
        Guid userId,
        GetItemsWithVouchersOfMonthRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting items with vouchers for month: {Month}, year: {Year}", request.Month, request.Year);

        try
        {
            var results = await GetItemsWithVouchersDataAsync(userId, request.Year, request.Month, cancellationToken);

            if (results.Count == 0)
            {
                _logger.LogInformation("No items with vouchers found for month: {Month}, year: {Year}", request.Month, request.Year);
                return _responseHandler.Success(new GetItemsWithVouchersOfMonthResponse
                {
                    Items = new List<GetItemsWithVouchersOfMonthResult>(),
                    TotalIncomeInMonth = 0,
                    TotalExpenseInMonth = 0,
                    TotalCount = 0
                }, "No items with vouchers found for the specified month and year.");
            }

            var response = new GetItemsWithVouchersOfMonthResponse
            {
                Items = results,
                TotalIncomeInMonth = results.Sum(i => i.VouchersTotalInValue),
                TotalExpenseInMonth = results.Sum(i => i.VouchersTotalOutValue),
                TotalCount = results.Count
            };

            _logger.LogInformation("Retrieved {ItemCount} items with vouchers for month: {Month}, year: {Year}",
                results.Count, request.Month, request.Year);
            return _responseHandler.Success(response, "Items with vouchers retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("GetItemsWithVouchersOfMonthAsync cancelled for Month: {Month}, Year: {Year}", request.Month, request.Year);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving items with vouchers for Month: {Month}, Year: {Year}", request.Month, request.Year);
            return _responseHandler.InternalServerError<GetItemsWithVouchersOfMonthResponse>("An error occurred while retrieving items with vouchers.");
        }
    }

    private async Task<List<GetItemsWithVouchersOfMonthResult>> GetItemsWithVouchersDataAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var startOfMonth = DateTime.SpecifyKind(new DateTime(year, month, 1), DateTimeKind.Utc);
        var startOfNextMonth = DateTime.SpecifyKind(startOfMonth.AddMonths(1), DateTimeKind.Utc);

        var itemsWithVouchers = await _context.Items
            .Where(i => i.Section.Category.Warehouse.UserId == userId &&
                i.ItemVouchers.Any(v => v.VoucherDate >= startOfMonth
                    && v.VoucherDate < startOfNextMonth))
            .Include(i => i.Section)
                .ThenInclude(s => s.Category)
            .AsNoTracking()
            .Select(i => new
            {
                Item = i,
                MonthVouchers = i.ItemVouchers
                    .Where(v => v.VoucherDate >= startOfMonth
                        && v.VoucherDate < startOfNextMonth)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var results = itemsWithVouchers.Select(x => new GetItemsWithVouchersOfMonthResult
        {
            Id = x.Item.Id,
            ItemCode = x.Item.ItemCode,
            PartNo = x.Item.PartNo,
            Description = x.Item.Description,
            SectionId = x.Item.SectionId,
            SectionName = x.Item.Section.Name,
            CategoryId = x.Item.Section.CategoryId,
            CategoryName = x.Item.Section.Category.Name,
            Unit = x.Item.Unit,
            VouchersTotalInQuantity = x.MonthVouchers.Sum(v => v.InQuantity),
            VouchersTotalOutQuantity = x.MonthVouchers.Sum(v => v.OutQuantity),
            VouchersTotalQuantity = x.MonthVouchers.Sum(v => v.InQuantity - v.OutQuantity),
            VouchersTotalInValue = x.MonthVouchers.Sum(v => v.InQuantity * v.UnitPrice),
            VouchersTotalOutValue = x.MonthVouchers.Sum(v => v.OutQuantity * v.UnitPrice),
            VouchersTotalValue = x.MonthVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice)
        })
        .OrderBy(i => i.SectionId)
            .ThenBy(i => i.ItemCode.Length)
                .ThenBy(i => i.ItemCode)
        .ToList();

        return results;
    }

    public async Task<Response<SearchItemsResponse>> SearchItemsAsync(
        Guid userId,
        SearchItemsRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching items with term: {SearchTerm}", request.SearchTerm);

        try
        {
            var query = _context.Items
                .AsNoTracking()
                .Where(i => i.Section.Category.Warehouse.UserId == userId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = $"%{request.SearchTerm.Trim()}%";
                query = query.Where(i =>
                    (i.ItemCode != null && EF.Functions.Like(i.ItemCode, searchTerm)) ||
                    (i.PartNo != null && EF.Functions.Like(i.PartNo, searchTerm)) ||
                    (i.Description != null && EF.Functions.Like(i.Description, searchTerm)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(i => i.Section.CreatedAt)
                    .ThenBy(i => i.ItemCode.Length)
                        .ThenBy(i => i.ItemCode)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new
                {
                    Item = i,
                    SectionName = i.Section.Name,
                    CategoryId = i.Section.CategoryId,
                    CategoryName = i.Section.Category.Name,
                    NetQuantity = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => v.InQuantity - v.OutQuantity) : 0,
                    NetValue = i.ItemVouchers != null ? i.ItemVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice) : 0m
                })
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
            {
                _logger.LogInformation("No items found with search term: {SearchTerm}", request.SearchTerm);
                return _responseHandler.Success(new SearchItemsResponse
                {
                    Items = new List<SearchItemsResult>(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    HasNextPage = false,
                    HasPreviousPage = false
                }, "No items found matching the search criteria.");
            }

            var itemResults = items.Select(x => new SearchItemsResult
            {
                Id = x.Item.Id,
                ItemCode = x.Item.ItemCode,
                PartNo = x.Item.PartNo,
                Description = x.Item.Description,
                UnitOfMeasure = x.Item.Unit,
                SectionId = x.Item.SectionId,
                SectionName = x.SectionName,
                CategoryId = x.CategoryId,
                CategoryName = x.CategoryName,
                OpeningQuantity = x.Item.OpeningQuantity,
                OpeningUnitPrice = x.Item.OpeningUnitPrice,
                OpeningDate = DateTime.SpecifyKind(x.Item.OpeningDate, DateTimeKind.Utc),
                AvailableQuantity = x.Item.OpeningQuantity + x.NetQuantity,
                AvailableValue = (x.Item.OpeningUnitPrice * x.Item.OpeningQuantity) + x.NetValue,
                CreatedAt = DateTime.SpecifyKind(x.Item.CreatedAt, DateTimeKind.Utc)
            }).ToList();

            var response = new SearchItemsResponse
            {
                Items = itemResults,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                HasNextPage = (request.PageNumber * request.PageSize) < totalCount,
                HasPreviousPage = request.PageNumber > 1
            };

            _logger.LogInformation("Found {ItemCount} items matching search term: {SearchTerm}",
                itemResults.Count, request.SearchTerm);
            return _responseHandler.Success(response, "Items retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SearchItemsAsync cancelled for SearchTerm: {SearchTerm}", request.SearchTerm);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching items with term: {SearchTerm}", request.SearchTerm);
            return _responseHandler.InternalServerError<SearchItemsResponse>("An error occurred while searching items.");
        }
    }

    public async Task<Response<CreateItemResponse>> CreateItemAsync(
        Guid userId,
        CreateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new item in section: {SectionId}", request.SectionId);

        try
        {
            var section = await _context.Sections
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SectionId
                    && s.Category.Warehouse.UserId == userId, cancellationToken);

            if (section == null)
            {
                _logger.LogWarning("Section: {SectionId} not found. Cannot create item.", request.SectionId);
                return _responseHandler.NotFound<CreateItemResponse>("Section not found");
            }

            var existingItem = await _context.Items
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.SectionId == request.SectionId
                    && i.ItemCode == request.ItemCode, cancellationToken);

            if (existingItem != null)
            {
                _logger.LogWarning("Item with code: {ItemCode} already exists in section: {SectionId}",
                    request.ItemCode, request.SectionId);
                return _responseHandler.BadRequest<CreateItemResponse>("An item with the same code already exists in this section.");
            }

            var newItem = _mapper.Map<Item>(request);
            newItem.Id = Guid.NewGuid();
            newItem.CreatedAt = DateTime.UtcNow;

            await _context.Items.AddAsync(newItem, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            var responseData = _mapper.Map<CreateItemResponse>(newItem);

            _logger.LogInformation("Item created successfully with Id: {ItemId} in section: {SectionId}",
                newItem.Id, request.SectionId);
            return _responseHandler.Success(responseData, "Item created successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("CreateItemAsync cancelled for Section: {SectionId}", request.SectionId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating item in section: {SectionId}", request.SectionId);
            return _responseHandler.InternalServerError<CreateItemResponse>("An error occurred while creating the item.");
        }
    }

    public async Task<Response<UpdateItemResponse>> UpdateItemAsync(
        Guid userId,
        UpdateItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating item: {ItemId}", request.Id);

        try
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.Id
                    && i.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Item: {Id} not found. Cannot update item.", request.Id);
                return _responseHandler.NotFound<UpdateItemResponse>("Item not found");
            }

            // Check for ItemCode conflict within the same section if code changed
            if (!string.Equals(item.ItemCode, request.ItemCode, StringComparison.OrdinalIgnoreCase))
            {
                var existingWithCode = await _context.Items
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.SectionId == item.SectionId
                            && i.ItemCode == request.ItemCode
                            && i.Id != item.Id, cancellationToken);

                if (existingWithCode != null)
                {
                    _logger.LogWarning("Item code conflict: {ItemCode} already exists in section {SectionId}", request.ItemCode, item.SectionId);
                    return _responseHandler.BadRequest<UpdateItemResponse>("An item with the same code already exists in this section.");
                }
            }

            _mapper.Map(request, item);

            await _context.SaveChangesAsync(cancellationToken);

            var responseData = _mapper.Map<UpdateItemResponse>(item);

            _logger.LogInformation("Item updated successfully with Id: {ItemId}", item.Id);
            return _responseHandler.Success(responseData, "Item updated successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("UpdateItemAsync cancelled for Item: {ItemId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating item with Id: {ItemId}", request.Id);
            return _responseHandler.InternalServerError<UpdateItemResponse>("An error occurred while updating the item.");
        }
    }

    public async Task<Response<DeleteItemResponse>> DeleteItemAsync(
        Guid userId,
        DeleteItemRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting item: {ItemId}", request.Id);

        try
        {
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == request.Id
                    && i.Section.Category.Warehouse.UserId == userId, cancellationToken);

            if (item == null)
            {
                _logger.LogWarning("Item: {ItemId} not found. Cannot delete item.", request.Id);
                return _responseHandler.NotFound<DeleteItemResponse>("Item not found");
            }

            var hasVouchers = await _context.ItemVouchers
                .AsNoTracking()
                .AnyAsync(iv => iv.ItemId == item.Id, cancellationToken);

            if (hasVouchers)
            {
                _logger.LogWarning("Item: {ItemId} has associated vouchers. Cannot delete item.", request.Id);
                return _responseHandler.BadRequest<DeleteItemResponse>("Cannot delete item with associated vouchers.");
            }

            _context.Items.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);

            var responseData = new DeleteItemResponse
            {
                Id = item.Id
            };

            _logger.LogInformation("Item deleted successfully with Id: {ItemId}", item.Id);
            return _responseHandler.Success(responseData, "Item deleted successfully.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DeleteItemAsync cancelled for Item: {ItemId}", request.Id);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting item with Id: {ItemId}", request.Id);
            return _responseHandler.InternalServerError<DeleteItemResponse>("An error occurred while deleting the item.");
        }
    }

    public async Task<byte[]> ExportMonthlyItemsToExcelAsync(
        Guid userId,
        GetItemsWithVouchersOfMonthRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting items with vouchers to Excel for month: {Month}, year: {Year}", request.Month, request.Year);

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var items = await GetItemsWithVouchersDataAsync(userId, request.Year, request.Month, cancellationToken);

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add($"Items_{request.Month:D2}_{request.Year}");

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
                worksheet.Cells[row, 4].Value = TranslateUnitToArabic(item.Unit);
                worksheet.Cells[row, 5].Value = item.CategoryName;
                worksheet.Cells[row, 6].Value = item.SectionName;
                worksheet.Cells[row, 7].Value = item.VouchersTotalInQuantity;
                worksheet.Cells[row, 8].Value = item.VouchersTotalOutQuantity;
                worksheet.Cells[row, 9].Value = item.VouchersTotalInValue;
                worksheet.Cells[row, 10].Value = item.VouchersTotalOutValue;

                // Apply styling to data rows
                using (var range = worksheet.Cells[row, 1, row, 10])
                {
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

            _logger.LogInformation("Excel file generated successfully for month: {Month}, year: {Year} with {ItemCount} items",
                request.Month, request.Year, items.Count);

            return package.GetAsByteArray();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("ExportMonthlyItemsToExcelAsync cancelled for Month: {Month}, Year: {Year}", request.Month, request.Year);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting items to Excel for Month: {Month}, Year: {Year}", request.Month, request.Year);
            throw;
        }
    }

    private static string TranslateUnitToArabic(UnitOfMeasure unit)
    {
        return unit switch
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
}