using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Entities.DTO.Items.GetItemsOfSection
{
    public class GetItemsOfSectionResult
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string PartNo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal OpeningQuantity { get; set; }
        public decimal OpeningValue { get; set; }
        public DateTime OpeningDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
