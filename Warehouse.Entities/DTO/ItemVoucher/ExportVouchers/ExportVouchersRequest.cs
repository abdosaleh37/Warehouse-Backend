using Warehouse.Entities.Utilities.Enums;

namespace Warehouse.Entities.DTO.ItemVoucher.ExportVouchers;

public class ExportVouchersRequest
{
    public VoucherType VoucherType { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
}
