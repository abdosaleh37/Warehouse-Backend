using Mapster;
using System.Linq;
using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
using Warehouse.Entities.DTO.Items.Update;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.Mappings;

public class ItemMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateItemRequest, Item>()
            .Map(dest => dest.SectionId, src => src.SectionId)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Unit, src => src.Unit)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningValue, src => src.OpeningValue)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .IgnoreNullValues(true);

        // Map update request onto existing Item
        config.NewConfig<UpdateItemRequest, Item>()
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Unit, src => src.Unit)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningValue, src => src.OpeningValue)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .IgnoreNullValues(true);

        config.NewConfig<Item, CreateItemResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SectionId, src => src.SectionId)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.UnitOfMeasure, src => src.Unit)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningValue, src => src.OpeningValue)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<Item, GetItemsOfSectionResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningValue, src => src.OpeningValue)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .Map(dest => dest.AvailableQuantity, src => (src.OpeningQuantity) + (src.ItemVouchers != null ? src.ItemVouchers.Sum(v => v.InQuantity - v.OutQuantity) : 0))
            .Map(dest => dest.AvailableValue, src => (src.OpeningValue * src.OpeningQuantity) + (src.ItemVouchers != null ? src.ItemVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice) : 0m))
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<Item, GetByIdResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Unit, src => src.Unit)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningValue, src => src.OpeningValue)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .Map(dest => dest.AvailableQuantity, src => (src.OpeningQuantity) + (src.ItemVouchers != null ? src.ItemVouchers.Sum(v => v.InQuantity - v.OutQuantity) : 0))
            .Map(dest => dest.AvailableValue, src => (src.OpeningValue * src.OpeningQuantity) + (src.ItemVouchers != null ? src.ItemVouchers.Sum(v => (v.InQuantity - v.OutQuantity) * v.UnitPrice) : 0m))
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.SectionId, src => src.SectionId)
            .Map(dest => dest.SectionName, src => src.Section.Name)
            .IgnoreNullValues(true);

        config.NewConfig<Item, UpdateItemResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SectionId, src => src.SectionId)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.UnitOfMeasure, src => src.Unit)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningValue, src => src.OpeningValue)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .IgnoreNullValues(true);
    }
}
