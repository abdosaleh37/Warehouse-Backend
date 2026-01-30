namespace Warehouse.Entities.DTO.ItemVoucher.CreateWithManyItems
{
    public class ItemData
    {
        public Guid ItemId { get; set; }
        public int InQuantity { get; set; } = 0;
        public int OutQuantity { get; set; } = 0;
        public decimal UnitPrice { get; set; } = 0;
        public string? Notes { get; set; }
    }
}
