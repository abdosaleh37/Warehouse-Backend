namespace Warehouse.Entities.DTO.ItemVoucher.GetVouchersOfItem
{
    public class GetVouchersOfItemResponse
    {
        public List<GetVouchersOfItemResult> Vouchers { get; set; } = new();
        public int TotalCount { get; set; }

        public Guid ItemId { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
        public int ItemAvailableQuantity { get; set; }
        public decimal ItemAvailableValue { get; set; }

        public int TotalInQuantity { get; set; }
        public decimal TotalInValue { get; set; }

        public int TotalOutQuantity { get; set; }
        public decimal TotalOutValue { get; set; }
    }
}
