namespace Warehouse.Entities.DTO.Items.GetItemsOfSection
{
    public class GetItemsOfSectionRequest
    {
        public Guid SectionId { get; set; }
        public string? SearchString { get; set; }
    }
}
