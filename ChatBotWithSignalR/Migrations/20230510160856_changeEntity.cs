using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBotWithSignalR.Migrations
{
    public partial class changeEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationFiles_Conversations_ConversationId",
                table: "ConversationFiles");

            migrationBuilder.DropColumn(
                name: "FromUserId",
                table: "ConversationFiles");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "ConversationFiles");

            migrationBuilder.DropColumn(
                name: "SendDate",
                table: "ConversationFiles");

            migrationBuilder.DropColumn(
                name: "ToUserId",
                table: "ConversationFiles");

            migrationBuilder.AlterColumn<int>(
                name: "ConversationId",
                table: "ConversationFiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationFiles_Conversations_ConversationId",
                table: "ConversationFiles",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConversationFiles_Conversations_ConversationId",
                table: "ConversationFiles");

            migrationBuilder.AlterColumn<int>(
                name: "ConversationId",
                table: "ConversationFiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "FromUserId",
                table: "ConversationFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "ConversationFiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendDate",
                table: "ConversationFiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ToUserId",
                table: "ConversationFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConversationFiles_Conversations_ConversationId",
                table: "ConversationFiles",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id");
        }
    }
}
