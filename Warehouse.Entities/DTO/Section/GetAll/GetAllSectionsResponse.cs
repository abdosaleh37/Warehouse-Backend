namespace Warehouse.Entities.DTO.Section.GetAll
{
    public class GetAllSectionsResponse
    {
        public List<GetAllSectionsResult> Sections { get; set; } = [];
        public int TotalSections { get; set; }
    }
}
