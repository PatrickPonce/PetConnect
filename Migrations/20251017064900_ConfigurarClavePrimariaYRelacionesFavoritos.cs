using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class ConfigurarClavePrimariaYRelacionesFavoritos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favoritos_Noticias_NoticiaId1",
                table: "Favoritos");

            migrationBuilder.DropIndex(
                name: "IX_Favoritos_NoticiaId1",
                table: "Favoritos");

            migrationBuilder.DropColumn(
                name: "NoticiaId1",
                table: "Favoritos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoticiaId1",
                table: "Favoritos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_NoticiaId1",
                table: "Favoritos",
                column: "NoticiaId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Favoritos_Noticias_NoticiaId1",
                table: "Favoritos",
                column: "NoticiaId1",
                principalTable: "Noticias",
                principalColumn: "Id");
        }
    }
}
