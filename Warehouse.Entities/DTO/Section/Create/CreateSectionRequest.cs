namespace Warehouse.Entities.DTO.Section.Create
{
    public class CreateSectionRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
    }
}
