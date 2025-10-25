using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class AgregadoEntidadesPetShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PetShops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    Ubicacion = table.Column<string>(type: "text", nullable: false),
                    DireccionCompleta = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    UrlLogo = table.Column<string>(type: "text", nullable: false),
                    UrlImagenPrincipal = table.Column<string>(type: "text", nullable: false),
                    Calificacion = table.Column<double>(type: "double precision", nullable: false),
                    Latitud = table.Column<double>(type: "double precision", nullable: false),
                    Longitud = table.Column<double>(type: "double precision", nullable: false),
                    UrlFacebook = table.Column<string>(type: "text", nullable: false),
                    UrlInstagram = table.Column<string>(type: "text", nullable: false),
                    MarcasPrincipales = table.Column<string>(type: "text", nullable: false),
                    Categorias = table.Column<string>(type: "text", nullable: false),
                    CompraOnline = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetShops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComentariosPetShop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    FechaComentario = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PetShopId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosPetShop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosPetShop_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComentariosPetShop_PetShops_PetShopId",
                        column: x => x.PetShopId,
                        principalTable: "PetShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoritosPetShop",
                columns: table => new
                {
                    PetShopId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritosPetShop", x => new { x.PetShopId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_FavoritosPetShop_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritosPetShop_PetShops_PetShopId",
                        column: x => x.PetShopId,
                        principalTable: "PetShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosPetShop_PetShopId",
                table: "ComentariosPetShop",
                column: "PetShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosPetShop_UsuarioId",
                table: "ComentariosPetShop",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosPetShop_UsuarioId",
                table: "FavoritosPetShop",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComentariosPetShop");

            migrationBuilder.DropTable(
                name: "FavoritosPetShop");

            migrationBuilder.DropTable(
                name: "PetShops");
        }
    }
}
