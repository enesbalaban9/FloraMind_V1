using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloraMind_V1.Migrations
{
    /// <inheritdoc />
    public partial class ResimSutunuEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Plants");
        }
    }
}
