using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Warehouse.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Warehouses_WarehouseId",
                table: "Sections");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "Sections",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Sections_WarehouseId",
                table: "Sections",
                newName: "IX_Sections_CategoryId");

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Category_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_WarehouseId_Name",
                table: "Category",
                columns: new[] { "WarehouseId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Category_CategoryId",
                table: "Sections",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Category_CategoryId",
                table: "Sections");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Sections",
                newName: "WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Sections_CategoryId",
                table: "Sections",
                newName: "IX_Sections_WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Warehouses_WarehouseId",
                table: "Sections",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
