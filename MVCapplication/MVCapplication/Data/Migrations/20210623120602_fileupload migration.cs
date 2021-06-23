using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCapplication.Data.Migrations
{
    public partial class fileuploadmigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "FileUploads",
                type: "varchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusMessage",
                table: "FileUploads",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "FileUploads");
        }
    }
}
