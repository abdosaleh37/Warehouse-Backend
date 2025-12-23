namespace Warehouse.Entities.DTO.Section.GetAll
{
    public class GetAllSectionsResult
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }
}
