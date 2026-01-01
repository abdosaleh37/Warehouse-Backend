using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIndexOfSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_Name",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Categories_WarehouseId_Name",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_WarehouseId",
                table: "Categories",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_WarehouseId",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Sections_Name",
                table: "Sections",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_WarehouseId_Name",
                table: "Categories",
                columns: new[] { "WarehouseId", "Name" },
                unique: true);
        }
    }
}
