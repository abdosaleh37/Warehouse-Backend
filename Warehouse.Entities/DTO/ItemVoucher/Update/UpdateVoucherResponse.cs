namespace Warehouse.Entities.DTO.ItemVoucher.Update
{
    public class UpdateVoucherResponse
    {
        public Guid Id { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public int InQuantity { get; set; }
        public int OutQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime VoucherDate { get; set; }
        public string? Notes { get; set; }
        public Guid ItemId { get; set; }
    }
}
