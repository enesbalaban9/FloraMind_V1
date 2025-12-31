using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloraMind_V1.Migrations
{
    /// <inheritdoc />
    public partial class FloraMindDB_V11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailSent",
                table: "UserPlants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextWateringDate",
                table: "UserPlants",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "WateringIntervalHours",
                table: "UserPlants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefaultWateringIntervalHours",
                table: "Plants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailSent",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "NextWateringDate",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "WateringIntervalHours",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "DefaultWateringIntervalHours",
                table: "Plants");
        }
    }
}
