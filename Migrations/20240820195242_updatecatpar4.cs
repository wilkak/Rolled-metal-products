using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolled_metal_products.Migrations
{
    /// <inheritdoc />
    public partial class updatecatpar4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "ProductParameters");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ProductParameters",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
