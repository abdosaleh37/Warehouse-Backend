namespace Warehouse.Entities.DTO.Section.Update
{
    public class UpdateSectionRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
