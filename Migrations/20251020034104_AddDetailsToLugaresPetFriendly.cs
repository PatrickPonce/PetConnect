using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailsToLugaresPetFriendly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UrlImagen",
                table: "LugaresPetFriendly",
                newName: "UrlImagenPrincipal");

            migrationBuilder.AddColumn<double>(
                name: "Calificacion",
                table: "LugaresPetFriendly",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "DireccionCompleta",
                table: "LugaresPetFriendly",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "LugaresPetFriendly",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlFacebook",
                table: "LugaresPetFriendly",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlInstagram",
                table: "LugaresPetFriendly",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlLogo",
                table: "LugaresPetFriendly",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Calificacion",
                table: "LugaresPetFriendly");

            migrationBuilder.DropColumn(
                name: "DireccionCompleta",
                table: "LugaresPetFriendly");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "LugaresPetFriendly");

            migrationBuilder.DropColumn(
                name: "UrlFacebook",
                table: "LugaresPetFriendly");

            migrationBuilder.DropColumn(
                name: "UrlInstagram",
                table: "LugaresPetFriendly");

            migrationBuilder.DropColumn(
                name: "UrlLogo",
                table: "LugaresPetFriendly");

            migrationBuilder.RenameColumn(
                name: "UrlImagenPrincipal",
                table: "LugaresPetFriendly",
                newName: "UrlImagen");
        }
    }
}
