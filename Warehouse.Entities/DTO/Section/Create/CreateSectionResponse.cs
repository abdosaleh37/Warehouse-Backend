namespace Warehouse.Entities.DTO.Section.Create
{
    public class CreateSectionResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
