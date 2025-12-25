using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace TaviLi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeospatialSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete all missions to ensure clean state for new schema
            migrationBuilder.Sql("DELETE FROM \"Missions\";");

            migrationBuilder.RenameColumn(
                name: "PickupAddress",
                table: "Missions",
                newName: "PickupAddress_FullAddress");

            migrationBuilder.RenameColumn(
                name: "DropoffAddress",
                table: "Missions",
                newName: "DropoffAddress_FullAddress");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.AddColumn<string>(
                name: "DropoffAddress_City",
                table: "Missions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DropoffAddress_HouseNumber",
                table: "Missions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "DropoffAddress_Location",
                table: "Missions",
                type: "geometry",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "DropoffAddress_Street",
                table: "Missions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress_City",
                table: "Missions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress_HouseNumber",
                table: "Missions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Point>(
                name: "PickupAddress_Location",
                table: "Missions",
                type: "geometry",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress_Street",
                table: "Missions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropoffAddress_City",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DropoffAddress_HouseNumber",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DropoffAddress_Location",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DropoffAddress_Street",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "PickupAddress_City",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "PickupAddress_HouseNumber",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "PickupAddress_Location",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "PickupAddress_Street",
                table: "Missions");

            migrationBuilder.RenameColumn(
                name: "PickupAddress_FullAddress",
                table: "Missions",
                newName: "PickupAddress");

            migrationBuilder.RenameColumn(
                name: "DropoffAddress_FullAddress",
                table: "Missions",
                newName: "DropoffAddress");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
