using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddProductosPetShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductosPetShop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric", nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    TipoProducto = table.Column<string>(type: "text", nullable: false),
                    QueryImagen = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosPetShop", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FavoritosProducto",
                columns: table => new
                {
                    ProductoPetShopId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritosProducto", x => new { x.ProductoPetShopId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_FavoritosProducto_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritosProducto_ProductosPetShop_ProductoPetShopId",
                        column: x => x.ProductoPetShopId,
                        principalTable: "ProductosPetShop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosProducto_UsuarioId",
                table: "FavoritosProducto",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoritosProducto");

            migrationBuilder.DropTable(
                name: "ProductosPetShop");
        }
    }
}
