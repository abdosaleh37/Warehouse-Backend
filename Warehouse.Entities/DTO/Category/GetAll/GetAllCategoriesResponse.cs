namespace Warehouse.Entities.DTO.Category.GetAll
{
    public class GetAllCategoriesResponse
    {
        public List<GetAllCategoriesResult> Categories { get; set; } = [];
        public int TotalCount { get; set; }
    }
}
