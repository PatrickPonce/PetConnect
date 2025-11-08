using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class SincronizacionDespuesDeMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoritosServicio",
                table: "FavoritosServicio");

            migrationBuilder.DropIndex(
                name: "IX_FavoritosServicio_ServicioId",
                table: "FavoritosServicio");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FavoritosServicio",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "ComentariosServicio",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoritosServicio",
                table: "FavoritosServicio",
                columns: new[] { "ServicioId", "UsuarioId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoritosServicio",
                table: "FavoritosServicio");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "FavoritosServicio",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<string>(
                name: "UsuarioId",
                table: "ComentariosServicio",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoritosServicio",
                table: "FavoritosServicio",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosServicio_ServicioId",
                table: "FavoritosServicio",
                column: "ServicioId");
        }
    }
}
