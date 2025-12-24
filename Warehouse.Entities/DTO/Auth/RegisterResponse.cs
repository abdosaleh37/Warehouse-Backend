namespace Warehouse.Entities.DTO.Auth
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid WarehouseId { get; set; }
        public string WarehouseName { get; set; } = string.Empty;
    }
}