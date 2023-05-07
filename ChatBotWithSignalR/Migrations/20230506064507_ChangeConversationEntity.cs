using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBotWithSignalR.Migrations
{
    public partial class ChangeConversationEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileType",
                table: "Conversations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileType",
                table: "Conversations");
        }
    }
}
