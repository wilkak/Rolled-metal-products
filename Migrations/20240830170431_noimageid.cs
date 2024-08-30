using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolled_metal_products.Migrations
{
    /// <inheritdoc />
    public partial class noimageid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Categories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageId",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageId",
                table: "Categories",
                type: "text",
                nullable: true);
        }
    }
}
