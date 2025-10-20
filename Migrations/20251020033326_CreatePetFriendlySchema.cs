using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class CreatePetFriendlySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LugaresPetFriendly",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ubicacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UrlImagen = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LugaresPetFriendly", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComentariosLugar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    FechaComentario = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LugarPetFriendlyId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComentariosLugar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComentariosLugar_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComentariosLugar_LugaresPetFriendly_LugarPetFriendlyId",
                        column: x => x.LugarPetFriendlyId,
                        principalTable: "LugaresPetFriendly",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoritosLugar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<string>(type: "text", nullable: false),
                    LugarPetFriendlyId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoritosLugar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoritosLugar_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoritosLugar_LugaresPetFriendly_LugarPetFriendlyId",
                        column: x => x.LugarPetFriendlyId,
                        principalTable: "LugaresPetFriendly",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosLugar_LugarPetFriendlyId",
                table: "ComentariosLugar",
                column: "LugarPetFriendlyId");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosLugar_UsuarioId",
                table: "ComentariosLugar",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosLugar_LugarPetFriendlyId",
                table: "FavoritosLugar",
                column: "LugarPetFriendlyId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosLugar_UsuarioId",
                table: "FavoritosLugar",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComentariosLugar");

            migrationBuilder.DropTable(
                name: "FavoritosLugar");

            migrationBuilder.DropTable(
                name: "LugaresPetFriendly");
        }
    }
}
