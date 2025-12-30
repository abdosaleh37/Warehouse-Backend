using System;
using System.Collections.Generic;
using System.Text;

namespace Warehouse.Entities.DTO.Items.GetItemsOfSection
{
    public class GetItemsOfSectionResult
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string? PartNo { get; set; }
        public string Description { get; set; } = string.Empty;
        public int OpeningQuantity { get; set; }
        public decimal OpeningValue { get; set; }
        public DateTime OpeningDate { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal AvailableValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
