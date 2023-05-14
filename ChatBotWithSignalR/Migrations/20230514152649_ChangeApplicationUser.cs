using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBotWithSignalR.Migrations
{
    public partial class ChangeApplicationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChatUser",
                schema: "Identity",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChatUser",
                schema: "Identity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
