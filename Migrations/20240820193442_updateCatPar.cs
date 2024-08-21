using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolled_metal_products.Migrations
{
    /// <inheritdoc />
    public partial class updateCatPar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryParameterId",
                table: "ProductParameters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductParameters_CategoryParameterId",
                table: "ProductParameters",
                column: "CategoryParameterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParameters_CategoryParameters_CategoryParameterId",
                table: "ProductParameters",
                column: "CategoryParameterId",
                principalTable: "CategoryParameters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductParameters_CategoryParameters_CategoryParameterId",
                table: "ProductParameters");

            migrationBuilder.DropIndex(
                name: "IX_ProductParameters_CategoryParameterId",
                table: "ProductParameters");

            migrationBuilder.DropColumn(
                name: "CategoryParameterId",
                table: "ProductParameters");
        }
    }
}
