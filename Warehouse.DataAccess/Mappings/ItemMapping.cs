using Mapster;
using Warehouse.Entities.DTO.Items.Create;
using Warehouse.Entities.DTO.Items.GetById;
using Warehouse.Entities.DTO.Items.GetItemsOfSection;
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

        config.NewConfig<Item, CreateItemResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.SectionId, src => src.SectionId)
            .Map(dest => dest.ItemCode, src => src.ItemCode)
            .Map(dest => dest.PartNo, src => src.PartNo)
            .Map(dest => dest.Description, src => src.Description)
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
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.SectionId, src => src.SectionId)
            .Map(dest => dest.SectionName, src => src.Section.Name)
            .IgnoreNullValues(true);
    }
}
