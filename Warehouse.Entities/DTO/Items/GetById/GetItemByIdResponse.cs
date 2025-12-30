using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.DTO.Items.GetById
{
    public class GetItemByIdResponse
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public string Description { get; set; } = string.Empty;
        public UnitOfMeasure Unit { get; set; } 
        public int OpeningQuantity { get; set; }
        public decimal OpeningValue { get; set; }
        public DateTime OpeningDate { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal AvailableValue { get; set; }
        public DateTime CreatedAt { get; set; }

        public Guid SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
    }
}