using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.DTO.Items.Update
{
    public class UpdateItemRequest
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public string Description { get; set; } = string.Empty;
        public UnitOfMeasure Unit { get; set; } = UnitOfMeasure.Piece;
        public int OpeningQuantity { get; set; } = 0;
        public decimal OpeningUnitPrice { get; set; } = 0;
        public DateTime OpeningDate { get; set; }
    }
}
