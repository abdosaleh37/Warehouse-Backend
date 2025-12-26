using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Entities.DTO.Category.Create
{
    public class CreateCategoryResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public Guid WarehouseId { get; set; }
    }
}
