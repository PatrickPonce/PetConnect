using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddLugarYGuarderiaDetalles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guarderia_detalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DireccionCompleta = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    ServicioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guarderia_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guarderia_detalles_servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lugar_pet_friendly_detalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Categoria = table.Column<string>(type: "text", nullable: false),
                    DireccionCompleta = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: false),
                    ServicioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lugar_pet_friendly_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lugar_pet_friendly_detalles_servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guarderia_detalles_ServicioId",
                table: "guarderia_detalles",
                column: "ServicioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lugar_pet_friendly_detalles_ServicioId",
                table: "lugar_pet_friendly_detalles",
                column: "ServicioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guarderia_detalles");

            migrationBuilder.DropTable(
                name: "lugar_pet_friendly_detalles");
        }
    }
}
