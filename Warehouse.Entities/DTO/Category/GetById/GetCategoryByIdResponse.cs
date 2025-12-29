namespace Warehouse.Entities.DTO.Category.GetById
{
    public class GetCategoryByIdResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int SectionCount { get; set; }
    }
}
