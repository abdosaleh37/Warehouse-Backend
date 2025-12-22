namespace Warehouse.Entities.DTO.Items.Create
{
    public class CreateItemResponse
    {
        public Guid Id { get; set; }
        public Guid SectionId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public string Description { get; set; } = string.Empty;
        public int OpeningQuantity { get; set; }
        public decimal OpeningValue { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
