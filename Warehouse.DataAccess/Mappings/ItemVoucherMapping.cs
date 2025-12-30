using Mapster;
using Warehouse.Entities.DTO.ItemVoucher.Create;
using Warehouse.Entities.DTO.ItemVoucher.GetById;
using Warehouse.Entities.DTO.ItemVoucher.GetVouchersOfItem;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.Mappings;

public class ItemVoucherMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ItemVoucher, GetVouchersOfItemResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.VoucherCode, src => src.VoucherCode)
            .Map(dest => dest.InQuantity, src => src.InQuantity)
            .Map(dest => dest.OutQuantity, src => src.OutQuantity)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice)
            .Map(dest => dest.InValue, src => src.InQuantity * src.UnitPrice)
            .Map(dest => dest.OutValue, src => src.OutQuantity * src.UnitPrice)
            .Map(dest => dest.VoucherDate, src => src.VoucherDate)
            .Map(dest => dest.Notes, src => src.Notes);

        config.NewConfig<ItemVoucher, GetVoucherByIdResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.VoucherCode, src => src.VoucherCode)
            .Map(dest => dest.InQuantity, src => src.InQuantity)
            .Map(dest => dest.OutQuantity, src => src.OutQuantity)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice)
            .Map(dest => dest.VoucherDate, src => src.VoucherDate)
            .Map(dest => dest.Notes, src => src.Notes)
            .Map(dest => dest.ItemId, src => src.ItemId)
            .Map(dest => dest.ItemDescription, src => src.Item.Description);

        config.NewConfig<CreateVoucherRequest, ItemVoucher>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.VoucherCode, src => src.VoucherCode)
            .Map(dest => dest.InQuantity, src => src.InQuantity)
            .Map(dest => dest.OutQuantity, src => src.OutQuantity)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice)
            .Map(dest => dest.VoucherDate, src => src.VoucherDate)
            .Map(dest => dest.Notes, src => src.Notes)
            .Map(dest => dest.ItemId, src => src.ItemId);

        config.NewConfig<ItemVoucher, CreateVoucherResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.VoucherCode, src => src.VoucherCode)
            .Map(dest => dest.InQuantity, src => src.InQuantity)
            .Map(dest => dest.OutQuantity, src => src.OutQuantity)
            .Map(dest => dest.UnitPrice, src => src.UnitPrice)
            .Map(dest => dest.VoucherDate, src => src.VoucherDate)
            .Map(dest => dest.Notes, src => src.Notes)
            .Map(dest => dest.ItemId, src => src.ItemId);
    }
}