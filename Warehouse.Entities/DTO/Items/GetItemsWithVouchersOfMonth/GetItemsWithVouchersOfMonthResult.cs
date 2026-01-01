using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth
{
    public class GetItemsWithVouchersOfMonthResult
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public string Description { get; set; } = string.Empty;

        public Guid SectionId { get; set; }
        public Guid CategoryId { get; set; }

        public string SectionName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public UnitOfMeasure Unit { get; set; }
        public int VouchersTotalQuantity { get; set; }
        public decimal VouchersTotalValue { get; set; }
    }
}
