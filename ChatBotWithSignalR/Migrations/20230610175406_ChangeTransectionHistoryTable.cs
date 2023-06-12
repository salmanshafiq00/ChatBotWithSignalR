using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBotWithSignalR.Migrations
{
    public partial class ChangeTransectionHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransectionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromGroupId = table.Column<int>(type: "int", nullable: true),
                    FromUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FromUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ToGroupId = table.Column<int>(type: "int", nullable: true),
                    NotifyUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NotifyUserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransectionId = table.Column<int>(type: "int", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", maxLength: 100, nullable: false),
                    TransectionType = table.Column<int>(type: "int", nullable: false),
                    TransectionTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSeen = table.Column<bool>(type: "bit", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    SeenDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransectionHistories", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransectionHistories");
        }
    }
}
