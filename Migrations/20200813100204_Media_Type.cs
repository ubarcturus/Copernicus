using Microsoft.EntityFrameworkCore.Migrations;

namespace Copernicus_Weather.Migrations
{
    public partial class Media_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Media_Type",
                table: "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Media_Type",
                table: "Apod");
        }
    }
}
