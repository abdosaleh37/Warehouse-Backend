using Warehouse.Entities.Entities;

namespace Warehouse.Entities.Shared.Helpers;

public static class FifoInventoryHelper
{
    public record FifoBatch(decimal UnitPrice, int AvailableQuantity);

    public static FifoBatch GetNextAvailableBatch(Item item, List<ItemVoucher> vouchers, int totalOutQuantity)
    {
        var remainingToConsume = totalOutQuantity;

        var openingRemaining = item.OpeningQuantity - remainingToConsume;
        if (openingRemaining > 0)
        {
            // Found available quantity in opening - check if next vouchers have same price
            var cumulativeQuantity = openingRemaining;
            var nextPrice = item.OpeningUnitPrice;
            
            remainingToConsume = 0; // Opening is not fully consumed
            
            // Check consecutive vouchers with same price
            foreach (var voucher in vouchers)
            {
                if (voucher.InQuantity > 0)
                {
                    var voucherRemaining = voucher.InQuantity - remainingToConsume;
                    if (voucherRemaining > 0 && voucher.UnitPrice == nextPrice)
                    {
                        // Same price - accumulate quantity
                        cumulativeQuantity += voucherRemaining;
                        remainingToConsume = 0;
                    }
                    else if (voucherRemaining > 0)
                    {
                        // Different price - stop accumulating
                        break;
                    }
                    else
                    {
                        remainingToConsume -= voucher.InQuantity;
                    }
                }
            }
            
            return new FifoBatch(nextPrice, cumulativeQuantity);
        }

        remainingToConsume -= item.OpeningQuantity;

        // Opening is fully consumed - find first available voucher
        foreach (var voucher in vouchers)
        {
            if (voucher.InQuantity > 0)
            {
                var voucherRemaining = voucher.InQuantity - remainingToConsume;
                if (voucherRemaining > 0)
                {
                    // Found first available batch - accumulate consecutive batches with same price
                    var cumulativeQuantity = voucherRemaining;
                    var nextPrice = voucher.UnitPrice;
                    
                    var foundFirst = false;
                    var tempRemaining = 0;
                    
                    foreach (var nextVoucher in vouchers)
                    {
                        if (nextVoucher == voucher)
                        {
                            foundFirst = true;
                            continue;
                        }
                        
                        if (foundFirst && nextVoucher.InQuantity > 0)
                        {
                            var nextVoucherRemaining = nextVoucher.InQuantity - tempRemaining;
                            if (nextVoucherRemaining > 0 && nextVoucher.UnitPrice == nextPrice)
                            {
                                // Same price - accumulate quantity
                                cumulativeQuantity += nextVoucherRemaining;
                                tempRemaining = 0;
                            }
                            else if (nextVoucherRemaining > 0)
                            {
                                // Different price - stop accumulating
                                break;
                            }
                            else
                            {
                                tempRemaining -= nextVoucher.InQuantity;
                            }
                        }
                    }
                    
                    return new FifoBatch(nextPrice, cumulativeQuantity);
                }
                remainingToConsume -= voucher.InQuantity;
            }
        }

        return new FifoBatch(item.OpeningUnitPrice, 0);
    }

    public static List<FifoBatch> GetBatchesForOutQuantity(
        Item item,
        List<ItemVoucher> vouchers,
        int totalOutQuantity,
        int requestedOutQuantity)
    {
        var batches = new List<FifoBatch>();
        var remainingToConsume = totalOutQuantity;
        var remainingToTake = requestedOutQuantity;

        var openingRemaining = item.OpeningQuantity - remainingToConsume;
        if (openingRemaining > 0 && remainingToTake > 0)
        {
            var takeFromOpening = Math.Min(openingRemaining, remainingToTake);
            batches.Add(new FifoBatch(item.OpeningUnitPrice, takeFromOpening));
            remainingToTake -= takeFromOpening;
        }

        remainingToConsume = Math.Max(0, remainingToConsume - item.OpeningQuantity);

        foreach (var voucher in vouchers)
        {
            if (remainingToTake <= 0)
                break;

            if (voucher.InQuantity > 0)
            {
                var voucherRemaining = voucher.InQuantity - remainingToConsume;
                if (voucherRemaining > 0)
                {
                    var takeFromVoucher = Math.Min(voucherRemaining, remainingToTake);
                    batches.Add(new FifoBatch(voucher.UnitPrice, takeFromVoucher));
                    remainingToTake -= takeFromVoucher;
                }
                remainingToConsume = Math.Max(0, remainingToConsume - voucher.InQuantity);
            }
        }

        // Merge consecutive batches with the same price
        var mergedBatches = new List<FifoBatch>();
        foreach (var batch in batches)
        {
            if (mergedBatches.Count > 0 && mergedBatches[^1].UnitPrice == batch.UnitPrice)
            {
                // Same price as previous batch - merge quantities
                var lastBatch = mergedBatches[^1];
                mergedBatches[^1] = new FifoBatch(lastBatch.UnitPrice, lastBatch.AvailableQuantity + batch.AvailableQuantity);
            }
            else
            {
                // Different price - add as new batch
                mergedBatches.Add(batch);
            }
        }

        return mergedBatches;
    }

    public static int GetAvailableQuantity(Item item, List<ItemVoucher> vouchers)
    {
        var netQuantity = vouchers.Sum(v => v.InQuantity - v.OutQuantity);
        return item.OpeningQuantity + netQuantity;
    }

    public static decimal GetAvailableValue(Item item, List<ItemVoucher> vouchers)
    {
        var netValue = vouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice);
        return (item.OpeningQuantity * item.OpeningUnitPrice) + netValue;
    }
}
