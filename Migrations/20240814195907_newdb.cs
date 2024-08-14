using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rolled_metal_products.Migrations
{
    /// <inheritdoc />
    public partial class newdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryParameter_Categories_CategoryId",
                table: "CategoryParameter");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductParameter_Products_ProductId",
                table: "ProductParameter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductParameter",
                table: "ProductParameter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryParameter",
                table: "CategoryParameter");

            migrationBuilder.RenameTable(
                name: "ProductParameter",
                newName: "ProductParameters");

            migrationBuilder.RenameTable(
                name: "CategoryParameter",
                newName: "CategoryParameters");

            migrationBuilder.RenameIndex(
                name: "IX_ProductParameter_ProductId",
                table: "ProductParameters",
                newName: "IX_ProductParameters_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryParameter_CategoryId",
                table: "CategoryParameters",
                newName: "IX_CategoryParameters_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductParameters",
                table: "ProductParameters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryParameters",
                table: "CategoryParameters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryParameters_Categories_CategoryId",
                table: "CategoryParameters",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParameters_Products_ProductId",
                table: "ProductParameters",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryParameters_Categories_CategoryId",
                table: "CategoryParameters");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductParameters_Products_ProductId",
                table: "ProductParameters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductParameters",
                table: "ProductParameters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryParameters",
                table: "CategoryParameters");

            migrationBuilder.RenameTable(
                name: "ProductParameters",
                newName: "ProductParameter");

            migrationBuilder.RenameTable(
                name: "CategoryParameters",
                newName: "CategoryParameter");

            migrationBuilder.RenameIndex(
                name: "IX_ProductParameters_ProductId",
                table: "ProductParameter",
                newName: "IX_ProductParameter_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryParameters_CategoryId",
                table: "CategoryParameter",
                newName: "IX_CategoryParameter_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductParameter",
                table: "ProductParameter",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryParameter",
                table: "CategoryParameter",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryParameter_Categories_CategoryId",
                table: "CategoryParameter",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductParameter_Products_ProductId",
                table: "ProductParameter",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
