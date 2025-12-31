using Mapster;
using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.DTO.Category.GetAll;
using Warehouse.Entities.DTO.Category.GetById;
using Warehouse.Entities.DTO.Category.Update;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.Mappings
{
    public class CategoryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Category, GetAllCategoriesResult>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            config.NewConfig<Category, GetCategoryByIdResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            config.NewConfig<Category, CreateCategoryResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.WarehouseId, src => src.WarehouseId);

            config.NewConfig<Category, UpdateCategoryResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.WarehouseId, src => src.WarehouseId);
        }
    }
}
