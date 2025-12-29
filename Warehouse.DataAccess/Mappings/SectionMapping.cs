using Mapster;
using Warehouse.Entities.DTO.Section.Create;
using Warehouse.Entities.DTO.Section.GetAll;
using Warehouse.Entities.DTO.Section.GetById;
using Warehouse.Entities.DTO.Section.GetSectionsOfCategory;
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
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        config.NewConfig<Section, GetSectionsOfCategoryResult>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        config.NewConfig<Section, GetSectionByIdResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.CategoryName, src => src.Category.Name)
            .Map(dest => dest.ItemCount, src => src.Items.Count);

        config.NewConfig<Section, CreateSectionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CategoryId, src => src.CategoryId)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<Section, UpdateSectionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.CategoryId, src => src.CategoryId);
    }
}
