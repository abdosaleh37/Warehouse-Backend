namespace Warehouse.Entities.DTO.ItemVoucher.GetMonthlyVouchersOfItem
{
    public class GetMonthlyVouchersOfItemResponse
    {
        public List<GetMonthlyVouchersOfItemResult> Vouchers { get; set; } = new();
        public int TotalCount { get; set; }

        public Guid ItemId { get; set; }
        public string ItemDescription { get; set; } = string.Empty;

        public int TotalInQuantity { get; set; }
        public decimal TotalInValue { get; set; }

        public int TotalOutQuantity { get; set; }
        public decimal TotalOutValue { get; set; }

        public int PreMonthItemAvailableQuantity { get; set; }
        public decimal PreMonthItemAvailableValue { get; set; }
        public int PostMonthItemAvailableQuantity { get; set; }
        public decimal PostMonthItemAvailableValue { get; set; }
    }
}
