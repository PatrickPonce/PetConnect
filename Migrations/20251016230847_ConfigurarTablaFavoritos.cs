using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarTablaFavoritos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    UsuarioId = table.Column<string>(type: "text", nullable: false),
                    NoticiaId = table.Column<int>(type: "integer", nullable: false),
                    FechaAgregado = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NoticiaId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => new { x.UsuarioId, x.NoticiaId });
                    table.ForeignKey(
                        name: "FK_Favoritos_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Favoritos_Noticias_NoticiaId",
                        column: x => x.NoticiaId,
                        principalTable: "Noticias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Noticias_NoticiaId1",
                        column: x => x.NoticiaId1,
                        principalTable: "Noticias",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_NoticiaId",
                table: "Favoritos",
                column: "NoticiaId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_NoticiaId1",
                table: "Favoritos",
                column: "NoticiaId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favoritos");
        }
    }
}
