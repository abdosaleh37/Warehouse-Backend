namespace Warehouse.Entities.DTO.ItemVoucher.CreateWithManyItems
{
    public class CreateVoucherWithManyItemsResponse
    {
        public Guid Id { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public DateTime VoucherDate { get; set; }
        public int ItemsCount { get; set; }
    }
}
