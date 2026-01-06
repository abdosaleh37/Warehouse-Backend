namespace Warehouse.Entities.DTO.Items.Search
{
    public class SearchItemsResponse
    {
        public List<SearchItemsResult> Items { get; set; } = new List<SearchItemsResult>();
        public int TotalCount { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
