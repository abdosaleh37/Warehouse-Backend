namespace Warehouse.Entities.DTO.ItemVoucher.CreateWithManyItems
{
    public class CreateVoucherWithManyItemsRequest
    {
        public string VoucherCode { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public List<ItemData> Items { get; set; } = new();
    }
}
