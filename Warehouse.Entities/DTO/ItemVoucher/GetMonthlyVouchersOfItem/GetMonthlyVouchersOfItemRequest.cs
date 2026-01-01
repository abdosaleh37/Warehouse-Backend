namespace Warehouse.Entities.DTO.ItemVoucher.GetMonthlyVouchersOfItem
{
    public class GetMonthlyVouchersOfItemRequest
    {
        public Guid ItemId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
