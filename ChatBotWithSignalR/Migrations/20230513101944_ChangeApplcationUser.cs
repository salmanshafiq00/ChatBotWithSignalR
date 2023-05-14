using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBotWithSignalR.Migrations
{
    public partial class ChangeApplcationUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "Identity",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "SeenDate",
                table: "Conversations",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SeenDate",
                table: "Conversations");
        }
    }
}
