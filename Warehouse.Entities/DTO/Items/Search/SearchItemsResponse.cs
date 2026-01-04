namespace Warehouse.Entities.DTO.Items.Search
{
    public class SearchItemsResponse
    {
        public List<SearchItemsResult> Items { get; set; } = new List<SearchItemsResult>();
        public int TotalCount { get; set; }
    }
}
