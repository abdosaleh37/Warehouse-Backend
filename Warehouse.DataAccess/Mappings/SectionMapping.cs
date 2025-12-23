using Mapster;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.DTO.Section.GetAll;
using Warehouse.Entities.DTO.Section.GetById;
using Warehouse.Entities.DTO.Section.Update;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.Mappings;

public class SectionMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Section, GetAllSectionsResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        config.NewConfig<Section, GetSectionByIdResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        config.NewConfig<Section, CreateSectionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<Section, UpdateSectionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);
    }
}
