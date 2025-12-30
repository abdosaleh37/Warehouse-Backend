namespace Warehouse.Entities.DTO.ItemVoucher
{
    public class GetVouchersOfItemResponse
    {
        public List<GetVouchersOfItemResult> Vouchers { get; set; } = new();
        public int TotalCount { get; set; }

        public Guid ItemId { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
    }
}
