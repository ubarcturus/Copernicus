using System;

namespace Copernicus_Weather.Migrations
{
    public partial class Apod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Apod",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    Explanation = table.Column<string>(nullable: true),
                    Copyright = table.Column<string>(nullable: true),
                    Url = table.Column<string>(nullable: true),
                    HdUrl = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Apod", x => x.Id); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Apod");
        }
    }
}