namespace Warehouse.Entities.DTO.Section.GetSectionsOfCategory
{
    public class GetSectionsOfCategoryResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ItemCount { get; set; }

    }
}
