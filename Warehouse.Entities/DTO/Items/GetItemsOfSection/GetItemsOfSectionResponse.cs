namespace Warehouse.Entities.DTO.Items.GetItemsOfSection
{
    public class GetItemsOfSectionResponse
    {
        public Guid SectionId { get; set; }
        public string SectionName { get; set; } = string.Empty;
        public List<GetItemsOfSectionResult> Items { get; set; } = new();
        public int TotalItemsCount { get; set; }
    }
}
