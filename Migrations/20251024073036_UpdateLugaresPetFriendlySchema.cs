using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLugaresPetFriendlySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoritosLugar",
                table: "FavoritosLugar");

            migrationBuilder.DropIndex(
                name: "IX_FavoritosLugar_LugarPetFriendlyId",
                table: "FavoritosLugar");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "FavoritosLugar");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoritosLugar",
                table: "FavoritosLugar",
                columns: new[] { "LugarPetFriendlyId", "UsuarioId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoritosLugar",
                table: "FavoritosLugar");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "FavoritosLugar",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoritosLugar",
                table: "FavoritosLugar",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosLugar_LugarPetFriendlyId",
                table: "FavoritosLugar",
                column: "LugarPetFriendlyId");
        }
    }
}
