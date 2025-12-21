using Mapster;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.Mappings;

public class SectionMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Section, CreateSectionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);

    }
}
