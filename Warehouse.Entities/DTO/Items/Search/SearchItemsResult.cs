using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.DTO.Items.Search
{
    public class SearchItemsResult
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public string Description { get; set; } = string.Empty;
        public UnitOfMeasure UnitOfMeasure { get; set; }

        public Guid SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;

        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int OpeningQuantity { get; set; }
        public decimal OpeningUnitPrice { get; set; }
        public DateTime OpeningDate { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal AvailableValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
