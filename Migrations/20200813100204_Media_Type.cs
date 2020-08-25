#region

using Microsoft.EntityFrameworkCore.Migrations;

#endregion

namespace Copernicus_Weather.Migrations
{
    public partial class Media_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                "Media_Type",
                "Apod",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "Media_Type",
                "Apod");
        }
    }
}