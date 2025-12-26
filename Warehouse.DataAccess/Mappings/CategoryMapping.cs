using Mapster;
using Warehouse.Entities.DTO.Category.Create;
using Warehouse.Entities.Entities;

namespace Warehouse.DataAccess.Mappings
{
    public class CategoryMapping : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Category, CreateCategoryResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Name, src => src.Name)
                .Map(dest => dest.CreatedAt, src => DateOnly.FromDateTime(src.CreatedAt))
                .Map(dest => dest.WarehouseId, src => src.WarehouseId);
        }
    }
}
