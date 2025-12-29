namespace Warehouse.Entities.DTO.Category.GetAll
{
    public class GetAllCategoriesResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public int SectionCount { get; set; }
    }
}
