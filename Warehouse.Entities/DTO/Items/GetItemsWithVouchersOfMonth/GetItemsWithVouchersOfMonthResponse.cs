namespace Warehouse.Entities.DTO.Items.GetItemsWithVouchersOfMonth
{
    public class GetItemsWithVouchersOfMonthResponse
    {
        public List<GetItemsWithVouchersOfMonthResult> Items { get; set; } = new();
        public double TotalIncomeInMonth { get; set; }
        public double TotalExpenseInMonth { get; set; }
        public int TotalCount { get; set; }
    }
}
