using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MVCapplication.Data.Migrations
{
    public partial class createdfiluploadtable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileUploads",
                columns: table => new
                {
                    F_ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "varchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "varchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "varchar(max)", nullable: true),
                    InsertedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsProcessed = table.Column<string>(type: "varchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileUploads", x => x.F_ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileUploads");
        }
    }
}
