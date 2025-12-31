using Mapster;
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
            .Map(dest => dest.OpeningUnitPrice, src => src.OpeningUnitPrice)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .IgnoreNullValues(true);

        config.NewConfig<UpdateItemRequest, Item>()
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Unit, src => src.Unit)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningUnitPrice, src => src.OpeningUnitPrice)
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
            .Map(dest => dest.OpeningUnitPrice, src => src.OpeningUnitPrice)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<Item, GetItemsOfSectionResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningUnitPrice, src => src.OpeningUnitPrice)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<Item, GetItemByIdResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Unit, src => src.Unit)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningUnitPrice, src => src.OpeningUnitPrice)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.SectionId, src => src.SectionId)
            .IgnoreNullValues(true);

        config.NewConfig<Item, UpdateItemResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SectionId, src => src.SectionId)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.UnitOfMeasure, src => src.Unit)
            .Map(dest => dest.OpeningQuantity, src => src.OpeningQuantity)
            .Map(dest => dest.OpeningUnitPrice, src => src.OpeningUnitPrice)
            .Map(dest => dest.OpeningDate, src => src.OpeningDate)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .IgnoreNullValues(true);
    }
}
