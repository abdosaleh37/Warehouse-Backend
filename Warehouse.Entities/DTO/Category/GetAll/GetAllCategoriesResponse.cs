namespace Warehouse.Entities.DTO.Category.GetAll
{
    public class GetAllCategoriesResponse
    {
        public List<GetAllCategoriesResult> Categories { get; set; } = [];
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
        public DateTime WarehouseCreateAt { get; set; }
        public int TotalCount { get; set; }
    }
}
