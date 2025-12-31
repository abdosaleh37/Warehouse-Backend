using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.DTO.Items.Update
{
    public class UpdateItemResponse
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public string Description { get; set; } = string.Empty;
        public UnitOfMeasure UnitOfMeasure { get; set; }
        public int OpeningQuantity { get; set; }
        public decimal OpeningUnitPrice { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
