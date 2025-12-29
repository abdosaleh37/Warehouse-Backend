namespace Warehouse.Entities.DTO.Category.Update
{
    public class UpdateCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly CreatedAt { get; set; }
        public Guid WarehouseId { get; set; }
    }
}
