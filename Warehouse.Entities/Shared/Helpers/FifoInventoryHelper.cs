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
            return new FifoBatch(item.OpeningUnitPrice, openingRemaining);
        }

        remainingToConsume -= item.OpeningQuantity;

        foreach (var voucher in vouchers)
        {
            if (voucher.InQuantity > 0)
            {
                var voucherRemaining = voucher.InQuantity - remainingToConsume;
                if (voucherRemaining > 0)
                {
                    return new FifoBatch(voucher.UnitPrice, voucherRemaining);
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

        return batches;
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
