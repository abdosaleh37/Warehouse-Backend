namespace Warehouse.Entities.DTO.Items.Search
{
    public class SearchItemsRequest
    {
        public string SearchTerm { get; set; } = string.Empty;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 30;
    }
}
