using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class CorregirModelosDeDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "lugar_pet_friendly_detalles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DireccionCompleta",
                table: "lugar_pet_friendly_detalles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "lugar_pet_friendly_detalles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "lugar_pet_friendly_detalles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "guarderia_detalles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DireccionCompleta",
                table: "guarderia_detalles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "guarderia_detalles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "GuarderiaId",
                table: "FavoritosServicio",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuarderiaId",
                table: "ComentariosServicio",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoritosServicio_GuarderiaId",
                table: "FavoritosServicio",
                column: "GuarderiaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComentariosServicio_GuarderiaId",
                table: "ComentariosServicio",
                column: "GuarderiaId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComentariosServicio_Guarderias_GuarderiaId",
                table: "ComentariosServicio",
                column: "GuarderiaId",
                principalTable: "Guarderias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoritosServicio_Guarderias_GuarderiaId",
                table: "FavoritosServicio",
                column: "GuarderiaId",
                principalTable: "Guarderias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComentariosServicio_Guarderias_GuarderiaId",
                table: "ComentariosServicio");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoritosServicio_Guarderias_GuarderiaId",
                table: "FavoritosServicio");

            migrationBuilder.DropIndex(
                name: "IX_FavoritosServicio_GuarderiaId",
                table: "FavoritosServicio");

            migrationBuilder.DropIndex(
                name: "IX_ComentariosServicio_GuarderiaId",
                table: "ComentariosServicio");

            migrationBuilder.DropColumn(
                name: "GuarderiaId",
                table: "FavoritosServicio");

            migrationBuilder.DropColumn(
                name: "GuarderiaId",
                table: "ComentariosServicio");

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "lugar_pet_friendly_detalles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DireccionCompleta",
                table: "lugar_pet_friendly_detalles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "lugar_pet_friendly_detalles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "lugar_pet_friendly_detalles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "guarderia_detalles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DireccionCompleta",
                table: "guarderia_detalles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "guarderia_detalles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
