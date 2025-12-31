namespace Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth
{
    public class GetItemsWithVouchersOfMonthResponse
    {
        public List<GetItemsWithVouchersOfMonthResult> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
