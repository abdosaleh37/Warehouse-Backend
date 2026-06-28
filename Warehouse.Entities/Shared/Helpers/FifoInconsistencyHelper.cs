using Warehouse.Entities.Entities;

namespace Warehouse.Entities.Shared.Helpers;

public static class FifoConsistencyHelper
{
    public static List<ItemVoucher> OrderChronologically(IEnumerable<ItemVoucher> vouchers)
    {
        return vouchers
            .OrderBy(v => v.VoucherDate)
                .ThenBy(v => v.OutQuantity)
                    .ThenBy(v => v.VoucherCode.Length)
                        .ThenBy(v => v.VoucherCode)
            .ToList();
    }

    public static bool WouldCauseNegativeBalance(Item item, List<ItemVoucher> orderedVouchers)
    {
        var runningQuantity = item.OpeningQuantity;

        foreach (var voucher in orderedVouchers)
        {
            runningQuantity += voucher.InQuantity - voucher.OutQuantity;
            if (runningQuantity < 0)
            {
                return true;
            }
        }

        return false;
    }

    public static int FindCutoffIndex(List<ItemVoucher> orderedVouchers, ItemVoucher referenceVoucher)
    {
        for (var i = 0; i < orderedVouchers.Count; i++)
        {
            if (orderedVouchers[i].Id == referenceVoucher.Id)
            {
                return i;
            }
        }

        throw new InvalidOperationException(
            "Reference voucher could not be located in the ordered voucher list. " +
            "Ensure it was merged into the list before calling FindCutoffIndex.");
    }

    public static (List<ItemVoucher> ToRemove, List<ItemVoucher> ToAdd) RecostFrom(
        Item item,
        List<ItemVoucher> orderedVouchers,
        int cutoffIndex)
    {
        var toRemove = new List<ItemVoucher>();
        var toAdd = new List<ItemVoucher>();

        var runningOutCursor = 0;
        for (var i = 0; i < cutoffIndex; i++)
        {
            runningOutCursor += orderedVouchers[i].OutQuantity;
        }

        var allVouchersForFifo = orderedVouchers;

        var affectedRange = orderedVouchers.Skip(cutoffIndex).ToList();

        var outGroupsByCode = affectedRange
            .Where(v => v.OutQuantity > 0)
            .GroupBy(v => v.VoucherCode)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalQuantity = g.Sum(v => v.OutQuantity),
                    EarliestRow = g.OrderBy(v => v.VoucherDate).First(),
                    OriginalRows = g.ToList()
                });

        var inVouchers = affectedRange.Where(v => v.OutQuantity <= 0).ToList();

        var outMarkers = outGroupsByCode.Values.Select(g => g.EarliestRow).ToList();

        var collapsedOrdered = FifoConsistencyHelper.OrderChronologically(
            outMarkers.Concat(inVouchers));

        foreach (var entry in collapsedOrdered)
        {
            if (entry.OutQuantity <= 0)
            {
                continue;
            }

            var group = outGroupsByCode[entry.VoucherCode];

            var batches = FifoInventoryHelper.GetBatchesForOutQuantity(
                item, allVouchersForFifo, runningOutCursor, group.TotalQuantity);

            toRemove.AddRange(group.OriginalRows);

            for (var batchIndex = 0; batchIndex < batches.Count; batchIndex++)
            {
                var batch = batches[batchIndex];
                var voucherDate = group.EarliestRow.VoucherDate.AddMicroseconds(batchIndex);

                toAdd.Add(new ItemVoucher
                {
                    Id = Guid.NewGuid(),
                    VoucherCode = entry.VoucherCode,
                    VoucherDate = voucherDate,
                    ItemId = group.EarliestRow.ItemId,
                    InQuantity = 0,
                    OutQuantity = batch.AvailableQuantity,
                    UnitPrice = batch.UnitPrice,
                    Notes = group.EarliestRow.Notes
                });
            }

            runningOutCursor += group.TotalQuantity;
        }

        return (toRemove, toAdd);
    }
}