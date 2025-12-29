namespace Warehouse.Entities.DTO.Category.GetAll
{
    public class GetAllCategoriesResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int SectionCount { get; set; }
    }
}
