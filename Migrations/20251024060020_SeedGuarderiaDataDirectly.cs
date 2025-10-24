using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class SeedGuarderiaDataDirectly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guarderias",
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
                    UrlInstagram = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guarderias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComentariosGuarderia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    FechaComentario = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuarderiaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosGuarderia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosGuarderia_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComentariosGuarderia_Guarderias_GuarderiaId",
                        column: x => x.GuarderiaId,
                        principalTable: "Guarderias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoritosGuarderia",
                columns: table => new
                {
                    GuarderiaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritosGuarderia", x => new { x.GuarderiaId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_FavoritosGuarderia_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritosGuarderia_Guarderias_GuarderiaId",
                        column: x => x.GuarderiaId,
                        principalTable: "Guarderias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosGuarderia_GuarderiaId",
                table: "ComentariosGuarderia",
                column: "GuarderiaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosGuarderia_UsuarioId",
                table: "ComentariosGuarderia",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosGuarderia_UsuarioId",
                table: "FavoritosGuarderia",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComentariosGuarderia");

            migrationBuilder.DropTable(
                name: "FavoritosGuarderia");

            migrationBuilder.DropTable(
                name: "Guarderias");
        }
    }
}
