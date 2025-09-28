using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddVeterinaria_adopcion_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    ImagenPrincipalUrl = table.Column<string>(type: "text", nullable: false),
                    FundacionNombre = table.Column<string>(type: "text", nullable: true),
                    DescripcionCorta = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "adopcion_detalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescripcionLarga = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    FacebookUrl = table.Column<string>(type: "text", nullable: true),
                    InstagramUrl = table.Column<string>(type: "text", nullable: true),
                    VideoUrl = table.Column<string>(type: "text", nullable: true),
                    ServicioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adopcion_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_adopcion_detalles_servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "petshop_detalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescripcionLarga = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    MarcasDestacadas = table.Column<string>(type: "text", nullable: true),
                    CategoriasProductos = table.Column<string>(type: "text", nullable: true),
                    OfreceCompraOnline = table.Column<bool>(type: "boolean", nullable: false),
                    ServicioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_petshop_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_petshop_detalles_servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "veterinaria_detalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DescripcionLarga = table.Column<string>(type: "text", nullable: false),
                    Direccion = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: false),
                    Horario = table.Column<string>(type: "text", nullable: true),
                    ServicioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_veterinaria_detalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_veterinaria_detalles_servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resenas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Autor = table.Column<string>(type: "text", nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    Puntuacion = table.Column<int>(type: "integer", nullable: false),
                    FechaResena = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VeterinariaDetalleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resenas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_resenas_veterinaria_detalles_VeterinariaDetalleId",
                        column: x => x.VeterinariaDetalleId,
                        principalTable: "veterinaria_detalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_adopcion_detalles_ServicioId",
                table: "adopcion_detalles",
                column: "ServicioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_petshop_detalles_ServicioId",
                table: "petshop_detalles",
                column: "ServicioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resenas_VeterinariaDetalleId",
                table: "resenas",
                column: "VeterinariaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_veterinaria_detalles_ServicioId",
                table: "veterinaria_detalles",
                column: "ServicioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adopcion_detalles");

            migrationBuilder.DropTable(
                name: "petshop_detalles");

            migrationBuilder.DropTable(
                name: "resenas");

            migrationBuilder.DropTable(
                name: "veterinaria_detalles");

            migrationBuilder.DropTable(
                name: "servicios");
        }
    }
}
