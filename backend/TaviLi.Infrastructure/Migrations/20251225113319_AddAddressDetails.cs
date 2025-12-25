using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaviLi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DropoffAddress_ApartmentNumber",
                table: "Missions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DropoffAddress_Entrance",
                table: "Missions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DropoffAddress_Floor",
                table: "Missions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PickupAddress_ApartmentNumber",
                table: "Missions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupAddress_Entrance",
                table: "Missions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PickupAddress_Floor",
                table: "Missions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropoffAddress_ApartmentNumber",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DropoffAddress_Entrance",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "DropoffAddress_Floor",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "PickupAddress_ApartmentNumber",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "PickupAddress_Entrance",
                table: "Missions");

            migrationBuilder.DropColumn(
                name: "PickupAddress_Floor",
                table: "Missions");
        }
    }
}
