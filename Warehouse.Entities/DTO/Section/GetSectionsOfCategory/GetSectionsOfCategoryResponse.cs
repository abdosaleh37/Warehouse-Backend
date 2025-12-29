namespace Warehouse.Entities.DTO.Section.GetSectionsOfCategory
{
    public class GetSectionsOfCategoryResponse
    {
        public List<GetSectionsOfCategoryResult> Sections { get; set; } = new();
        public int TotalCount { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
