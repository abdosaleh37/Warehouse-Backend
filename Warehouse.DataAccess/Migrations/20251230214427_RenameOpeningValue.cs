using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameOpeningValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpeningValue",
                table: "Items",
                newName: "OpeningUnitPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpeningUnitPrice",
                table: "Items",
                newName: "OpeningValue");
        }
    }
}
