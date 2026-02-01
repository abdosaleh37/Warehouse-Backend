namespace Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth
{
    public class GetItemsWithVouchersOfMonthResponse
    {
        public List<GetItemsWithVouchersOfMonthResult> Items { get; set; } = new();
        public decimal TotalIncomeInMonth { get; set; }
        public decimal TotalExpenseInMonth { get; set; }
        public int TotalCount { get; set; }
    }
}
