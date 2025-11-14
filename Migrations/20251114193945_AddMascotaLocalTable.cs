using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddMascotaLocalTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mascotas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Raza = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Edad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Genero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UrlImagen = table.Column<string>(type: "text", nullable: true),
                    Temperamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Contacto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaPublicacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mascotas", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mascotas");
        }
    }
}
