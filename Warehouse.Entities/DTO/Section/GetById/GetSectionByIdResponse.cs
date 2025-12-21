using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Entities.DTO.Section.GetById
{
    public class GetSectionByIdResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
