using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCapplication.Data.Migrations
{
    public partial class deletedstatusmessagecolumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "FileUploads");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusMessage",
                table: "FileUploads",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
