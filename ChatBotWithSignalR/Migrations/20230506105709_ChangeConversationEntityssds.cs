using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBotWithSignalR.Migrations
{
    public partial class ChangeConversationEntityssds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ConversationFiles_ConversationId",
                table: "ConversationFiles",
                column: "ConversationId");

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

            migrationBuilder.DropIndex(
                name: "IX_ConversationFiles_ConversationId",
                table: "ConversationFiles");
        }
    }
}
