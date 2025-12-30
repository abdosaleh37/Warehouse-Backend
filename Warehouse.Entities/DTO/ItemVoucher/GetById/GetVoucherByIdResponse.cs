namespace Warehouse.Entities.DTO.ItemVoucher.GetById
{
    public class GetVoucherByIdResponse
    {
        public Guid Id { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public int InQuantity { get; set; }
        public int OutQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime VoucherDate { get; set; }
        public string? Notes { get; set; }

        public Guid ItemId { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
    }
}
