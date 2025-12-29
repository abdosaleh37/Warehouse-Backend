namespace Warehouse.Entities.DTO.Section.Update
{
    public class UpdateSectionResponse
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
