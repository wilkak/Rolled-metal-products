using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolled_metal_products.Migrations
{
    /// <inheritdoc />
    public partial class addProductToDb4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Diameter",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExternalDiameter",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Stamp",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Standard",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Thickness",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfAlloy",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Width",
                table: "Products",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Diameter",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExternalDiameter",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Stamp",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Standard",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Thickness",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "TypeOfAlloy",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Products");
        }
    }
}
